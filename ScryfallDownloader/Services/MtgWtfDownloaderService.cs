using HtmlAgilityPack;
using ScryfallDownloader.Data;
using ScryfallDownloader.Extensions;
using System.Globalization;

namespace ScryfallDownloader.Services
{
    public class MtgWtfDownloaderService
    {
        private readonly HttpClient _httpClient;
        private readonly DataService _db;
        private readonly List<string> _mainSets = new() { "core", "expansion" };

        public MtgWtfDownloaderService(HttpClient httpClient, DataService db)
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri("https://mtg.wtf/deck");
            _db = db;
        }

        public async Task Download()
        {
            var settings = await _db.LoadSettings();

            var links = await GetLinks();

            var deckIndex = 1;
            foreach (var link in links)
            {
                await CreateDeck(link, deckIndex);
                deckIndex++;

                settings.WTFDeck = link;
                await _db.SaveSettings(settings);
            }
        }

        private async Task<List<string>> GetLinks()
        {
            var response = await _httpClient.GetAsync(_httpClient.BaseAddress);
            response.EnsureSuccessStatusCode();
            var data = await response.Content.ReadAsStringAsync();

            var doc = new HtmlDocument();
            doc.LoadHtml(data);

            return doc.DocumentNode.SelectNodes("//ul/li/a").Select(i => i.GetAttributeValue("href", "").Replace("/deck", "")).ToList();
        }

        private async Task CreateDeck(string deckUrl, int deckIndex)
        {
            var url = $"{_httpClient.BaseAddress}{deckUrl}/download";
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var dataString = await response.Content.ReadAsStringAsync();
            var data = dataString.SplitToLines();

            var deckName = data.FirstOrDefault(l => l.ToLower().StartsWith("// name:")).Replace("// NAME: ", "").Trim();
            var deckDate = DateTime.ParseExact(data.FirstOrDefault(l => l.ToLower().StartsWith("// date:")).Replace("// DATE: ", "").Trim(), "yyyy-MM-dd", CultureInfo.InvariantCulture);
            var extraDescription = data.FirstOrDefault(l => l.ToLower().StartsWith("// display:"))?.Replace("// DISPLAY: ", "").Trim();

            if (await _db.CheckDeckExists(deckName, "MTG.WTF", "MTG.WTF", deckDate)) return;

            var setCode = deckUrl.Split("/", StringSplitOptions.RemoveEmptyEntries)[0];
            var set = await _db.GetSet(setCode);

            bool isMainOnly = true;
            if (set != null) { isMainOnly = _mainSets.Contains(set.SetType.Name.ToLower()); }
            bool isCommanderDeck = data.Any(l => l.ToLower().Contains("commander:"));

            Deck deck = new()
            {
                Name = deckName,
                Description = $"Set: {setCode}\nUrl: {url}\n{extraDescription ?? ""}",
                Author = new Author() { Name = "MTG.WTF" },
                Source = new Source() { Name = "MTG.WTF", Url = _httpClient.BaseAddress },
                Format = new Format() { Name = isCommanderDeck ? "commanderPrecons" : "precons" },
                CreateDate = deckDate,
                UpdateDate = DateTime.Now,
            };

            var createdDeck = await _db.Create(deck);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"{deckIndex} | {createdDeck.DeckId}: {deckName} | {set.Code}/{set.ForgeCode}{(isCommanderDeck ? " | ::Comander" : "")}");
            Console.ResetColor();

            List<DeckCard> cards = new();
            var section = "mainboard";

            foreach (var line in data)
            {
                if (line.StartsWith("//")) continue;

                if (line.ToLower().StartsWith("commander:"))
                {
                    var commanderName = line.ToLower().Replace("commander: ", "").Trim().Split(" ", 2, StringSplitOptions.RemoveEmptyEntries)[1];
                    var commander = await _db.GetLatestCard(commanderName, DateOnly.FromDateTime(deckDate), false);
                    if (commander != null) { await _db.AddCommandersToDeck(createdDeck.DeckId, commander.CardId); continue; }
                }

                var groups = line.Split(" ", 2, StringSplitOptions.RemoveEmptyEntries);

                if (groups.Length < 2)
                {
                    section = "line";
                    if (!line.ToLower().Contains("sideboard")) Console.WriteLine(line);
                    continue;
                }

                var quantity = groups[0].ParseToInt();
                var cardName = groups[1];
                var cardEntity = await _db.GetLatestCard(cardName, DateOnly.FromDateTime(deckDate), isMainOnly);
                if (cardEntity != null)
                {
                    var deckCardIsSideboard = section != "mainboard";
                    if (cards.Any(c => c.CardId == cardEntity.CardId && c.IsSideboard == deckCardIsSideboard))
                    {
                        cards.FirstOrDefault(c => c.CardId == cardEntity.CardId && c.IsSideboard == deckCardIsSideboard).Quantity += quantity;
                    }
                    else
                    {
                        DeckCard card = new()
                        {
                            DeckId = createdDeck.DeckId,
                            CardId = cardEntity.CardId,
                            Quantity = quantity,
                            IsSideboard = deckCardIsSideboard,
                        };
                        cards.Add(card);
                    }
                }
                else
                {
                    if (deck.MissingCards == null) deck.MissingCards = new List<string>();
                    deck.MissingCards.Add(cardName);
                }
            }

            if (deck.MissingCards != null) await _db.UpdateDeckMissingCards(createdDeck.DeckId, deck.MissingCards);

            await _db.AddCardsToDeck(createdDeck, cards);
        }
    }
}
