using ScryfallDownloader.Data;
using ScryfallDownloader.Extensions;
using System.Text.RegularExpressions;

namespace ScryfallDownloader.Services
{
    public class DeckGeneratorService
    {
        private readonly DataService _db;
        private readonly List<string> _specialLayouts = new() { "vanguard", "planar", "scheme" };

        public DeckGeneratorService(DataService db)
        {
            _db = db;
        }
        public async Task<List<DeckCard>?> ParseImportLines(string importText, DateOnly setDate, bool isMainOnly)
        {
            if (string.IsNullOrWhiteSpace(importText)) return null;
            var section = "mainboard";
            Regex separators = new Regex(@"[\[\](|)]", RegexOptions.Compiled);

            List<DeckCard> cards = new();

            foreach (var line in importText.Split("\n"))
            {
                if (string.IsNullOrWhiteSpace(line)) continue;

                var compOpt = StringComparison.InvariantCultureIgnoreCase;
                if (line.StartsWith("sideboard", compOpt)) { section = "sideboard"; continue; }
                if (line.StartsWith("commander", compOpt)) { section = "commander"; continue; }


                if (!separators.IsMatch(line))
                {

                    int quantity;
                    string name;
                    if (line.StartsWith("sb:", compOpt) || line.StartsWith("cm:", compOpt))
                    {
                        var groups = line.Split(' ', 3, StringSplitOptions.RemoveEmptyEntries);
                        section = groups[0].ToLower() switch
                        {
                            "sb:" => "sideboard",
                            "cm:" => "commander",
                            _ => "mainboard",
                        };
                        quantity = groups[1].ParseToInt();
                        if (quantity == int.MinValue) { Console.WriteLine($"Couldn't parse deck line: {line}"); continue; }
                        name = groups[2];
                        cards.Add(await GenerateDeckCard(quantity, name, section == "sideboard", setDate, isMainOnly));
                    }
                    else
                    {
                        var groups = line.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
                        quantity = groups[0].ParseToInt();
                        if (quantity == int.MinValue) { Console.WriteLine($"Couldn't parse deck line: {line}"); continue; }
                        name = groups[1];
                        cards.Add(await GenerateDeckCard(quantity, name, section == "sideboard", setDate, isMainOnly));
                    }
                }
            }
            return cards;
        }

        private async Task<DeckCard?> GenerateDeckCard(int quantity, string name, bool isSideboard, DateOnly setDate, bool isMainOnly, string? set = null)
        {
            var cardEntity = string.IsNullOrWhiteSpace(set) ? await _db.GetLatestCard(name, setDate, isMainOnly) : await _db.GetCard(name, set);
            if (cardEntity == null) return null;

            return new DeckCard() { CardId = cardEntity.CardId, Card = cardEntity, IsSideboard = isSideboard, Quantity = quantity };
        }

        /// <summary>
        /// Generate deck text in Forge's scheme, including sections like [Metadata], [Mainboard], etc.
        /// </summary>
        /// <param name="deckId"></param>
        /// <returns>Tuple (string, string):<br/> Item1 is the deck's name.<br /> Item2 is the generated string with cards and other info.</returns>
        public async Task<(string, string)> GenerateDeck(int deckId)
        {
            var settings = await _db.LoadSettings();

            var deck = await _db.GetDeck(deckId);
            if (deck == null) return (string.Empty, string.Empty);
            if (deck.Cards.Any(c => !c.Card.IsImplemented))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Deck contains unimplemented cards: ID:{deck.DeckId} | {deck.Name}\nCanceling generation...");
                Console.ResetColor();
                //foreach (var card in deck.Cards)
                //{
                //    if (!card.Card.IsImplemented) Console.WriteLine($"{card.CardId}: {card.Card.Name}");
                //}
                settings.MissingCardDecks ??= new List<string>();
                settings.MissingCardDecks.Add(deck.DeckId.ToString());
                await _db.SaveSettings(settings);
                return (string.Empty, string.Empty);
            }
            if (deck.Cards.Any(c => c.Card.Set.ForgeCode == null))
            {
                Console.WriteLine($"Deck contains missing editions: ID:{deck.DeckId} | {deck.Name}\nTrying to find other prints for missing cards...");
                var newDeck = new List<DeckCard>();
                foreach (var card in deck.Cards)
                {
                    if (card.Card.Set.ForgeCode == null)
                    {
                        Console.WriteLine($"{card.CardId}: {card.Card.Name} | {card.Card.Set.Code}");
                        var newCard = await _db.GetLatestCard(card.Card.Name, DateOnly.FromDateTime(deck.CreateDate), false);
                        if (newCard != null && newCard.Set.ForgeCode != null) { newDeck.Add(new DeckCard() { Card = newCard, Quantity = card.Quantity, IsSideboard = card.IsSideboard }); }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine($"Card not found:\nID:{card.Card.CardId} | {card.Card.Name}\nCanceling generation...");
                            Console.ResetColor();

                            settings.MissingCardDecks ??= new List<string>();
                            settings.MissingCardDecks.Add(deck.DeckId.ToString());
                            await _db.SaveSettings(settings);
                            return (string.Empty, string.Empty);
                        }
                    }
                    else newDeck.Add(card);
                }
                deck.Cards = newDeck;
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Generating Deck: {deck.DeckId} | {deck.Name}");
            Console.ResetColor();

            List<DeckCard> specialCards = deck.Cards.Where(c => _specialLayouts.Contains(c.Card.Layout.Name) || c.Card.Type.ToLower().StartsWith("conspiracy") || c.Card.Type.ToLower().StartsWith("Dungeon")).ToList();

            deck.Cards = deck.Cards.Except(specialCards).ToList();

            List<DeckCard> mainCards = deck.Cards.Where(c => c.IsSideboard == false).ToList();
            List<DeckCard> sideCards = deck.Cards.Where(c => c.IsSideboard == true).ToList();

            Card? commander = null;
            if (deck.Commander != null)
            {
                commander = await _db.GetLatestCard(deck.Commander.Name, DateOnly.FromDateTime(deck.CreateDate), false);
                if (commander == null)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"Commander card not implemented:\n{commander.CardId}: {commander.Name}\nDeck ID:{deck.DeckId}");
                    Console.ResetColor();
                    return (string.Empty, string.Empty);
                }

                if (commander.Name.Contains(" // ")) commander.Name = commander.Name.ParseSplitCardname();
            }

            return (deck.Name, await GenerateDeckString(deck.Name, mainCards, sideCards, specialCards, commander));
        }

        private async Task<string> GenerateDeckString(string deckName, List<DeckCard> mainCards, List<DeckCard>? sideCards = null, List<DeckCard>? specialCards = null, Card? commander = null)
        {
            var deckString = string.Empty;

            deckString += $"[Metadata]\nName={deckName}\n";
            deckString += $"[Main]\n{string.Join("\n", await GenerateCardList(mainCards))}";

            if (sideCards.Count > 0)
                deckString += $"\n[Sideboard]\n{string.Join("\n", await GenerateCardList(sideCards))}";

            if (commander != null)
                deckString += $"\n[Commander]\n1 {commander.Name}|{commander.Set.ForgeCode.ToUpper()}";

            if (specialCards != null)
            {
                List<DeckCard> vanguard = new();
                List<DeckCard> planar = new();
                List<DeckCard> scheme = new();
                List<DeckCard> conspiracy = new();
                List<DeckCard> dungeon = new();

                foreach (var card in specialCards)
                {
                    if (card.Card.Layout.Name == "vanguard") vanguard.Add(card);
                    if (card.Card.Layout.Name == "planar") planar.Add(card);
                    if (card.Card.Layout.Name == "scheme") scheme.Add(card);
                    if (card.Card.Type.ToLower().StartsWith("conspiracy")) conspiracy.Add(card);
                    if (card.Card.Type.ToLower().StartsWith("dungeon")) dungeon.Add(card);
                }

                if (vanguard.Count > 0) deckString += $"[Avatar]{string.Join("\n", await GenerateCardList(vanguard))}";
                if (planar.Count > 0) deckString += $"[Planes]{string.Join("\n", await GenerateCardList(planar))}";
                if (scheme.Count > 0) deckString += $"[Schemes]{string.Join("\n", await GenerateCardList(scheme))}";
                if (conspiracy.Count > 0) deckString += $"[Conspiracy]{string.Join("\n", await GenerateCardList(conspiracy))}";
                if (dungeon.Count > 0) deckString += $"[Dungeon]{string.Join("\n", await GenerateCardList(dungeon))}";
            }

            return deckString;
        }

        private async Task<List<string>?> GenerateCardList(List<DeckCard> cards)
        {
            List<string> cardlist = new();


            var setIds = cards.Select(c => c.Card.Set.SetId).Distinct().ToList();
            List<Set> sets = new();
            foreach (var set in setIds)
            {
                sets.Add(await _db.GetSet(set, true));
            }

            foreach (var card in cards)
            {
                if (card.Card.Set == null || card.Card.Set.ForgeCode == null) return null;

                var cardName = card.Card.Name.RemoveDiacritics();
                if (cardName.Contains(" // ") && card.Card.Layout.Name != "split") cardName = cardName.ParseSplitCardname();

                if (card.Quantity == 1) { cardlist.Add($"1 {cardName}|{card.Card.Set.ForgeCode.ToUpper()}"); continue; }

                var setCards = sets.FirstOrDefault(s => s.SetId == card.Card.Set.SetId).Cards.Where(c => c.Name == card.Card.Name).ToList();
                if (setCards.Count == 1) { cardlist.Add($"{card.Quantity} {cardName}|{card.Card.Set.ForgeCode.ToUpper()}"); continue; }

                var variations = CalculateVariations(card.Quantity, setCards.Count);

                for (var i = 1; i <= variations.Count; i++)
                {
                    if (variations[i - 1] == 0) break;
                    cardlist.Add($"{variations[i - 1]} {cardName}|{card.Card.Set.ForgeCode.ToUpper()}|{i}");
                }
            }

            return cardlist;
        }

        private List<int> CalculateVariations(int quantity, int variations)
        {
            int remainder;
            int division = Math.DivRem(quantity, variations, out remainder);
            List<int> result = new();

            for (var i = 0; i < variations; i++)
            {
                result.Add(i < remainder ? division + 1 : division);
            }
            return result;
        }

        //private List<int> CalculateVariations(int quantity, int variations) => Enumerable.Range(0, quantity / variations).Select(n => quantity / variations + ((quantity % variations) <= n ? 0 : 1)).ToList();
    }
}
