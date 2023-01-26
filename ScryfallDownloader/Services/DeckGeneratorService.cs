using ScryfallDownloader.Data;

namespace ScryfallDownloader.Services
{
    public class DeckGeneratorService
    {
        private readonly DataService _db;

        public DeckGeneratorService(DataService db)
        {
            _db = db;
        }

        public async Task<string> GenerateDeck(int deckId)
        {
            var deck = await _db.GetDeck(deckId);
            if (deck == null) return string.Empty;
            if (deck.Cards.Any(c => c.Card.Set.ForgeCode == null))
            {
                Console.WriteLine($"Deck contains unimplemented cards: ID:{deck.DeckId} | {deck.Name}");
                foreach (var card in deck.Cards) { if (card.Card.Set.ForgeCode == null) Console.WriteLine($"{card.CardId}: {card.Card.Name}"); }
                return string.Empty;
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Generating Deck: {deck.DeckId} | {deck.Name}");
            Console.ResetColor();

            deck.Cards = await GenerateVariations(deck);

            List<DeckCard> mainCards = deck.Cards.Where(c => c.IsSideboard == false).ToList();
            var mainboard = string.Join("\n", mainCards.Select(c => $"{c.Quantity} {c.Card.Name}|{c.Card.Set.ForgeCode.ToUpper()}").ToArray());

            List<DeckCard> sideCards = deck.Cards.Where(c => c.IsSideboard == true).ToList();
            var sideboard = string.Join("\n", sideCards.Select(c => $"{c.Quantity} {c.Card.Name}|{c.Card.Set.ForgeCode.ToUpper()}").ToArray());

            Card? commander = null;
            if (deck.Commander != null) commander = deck.Commander;
            if (commander.Set.ForgeCode == null) { Console.WriteLine($"Commander card not implemented: ID:{commander.CardId} | {commander.Name}"); return string.Empty; }

            return $"[metadata]\nName={deck.Name}\n[main]\n{mainboard}{(sideCards.Count > 0 ? $"\n[sideboard]\n{sideboard}" : "")}{(commander != null ? $"\n[commander]\n1 {commander.Name}|{commander.Set.ForgeCode.ToUpper()}" : "")}";
        }

        private async Task<List<DeckCard>> GenerateVariations(Deck deck)
        {
            List<DeckCard> newDecklist = new();

            var setIds = deck.Cards.Select(c => c.Card.Set.SetId).Distinct().ToList();
            List<Set> sets = new();
            foreach (var set in setIds)
            {
                sets.Add(await _db.GetSet(set, true));
            }

            foreach (var card in deck.Cards)
            {
                if (card.Quantity == 1) { newDecklist.Add(card); continue; }

                var setCards = sets.FirstOrDefault(s => s.SetId == card.Card.Set.SetId).Cards.Where(c => c.Name == card.Card.Name).ToList();
                if (setCards.Count == 1) { newDecklist.Add(card); continue; }

                var variations = CalculateVariations(card.Quantity, setCards.Count);

                for (var i = 1; i <= variations.Count; i++)
                {

                }
            }

            return newDecklist;
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
