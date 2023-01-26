using ScryfallDownloader.Data;
using ScryfallDownloader.Extensions;
using System.Text.Json.Nodes;

namespace ScryfallDownloader.Services
{
    public class ArchidektDownloaderService
    {
        private readonly HttpClient _httpClient;
        private readonly DataService _db;
        private DateTime _startTime;

        public ArchidektDownloaderService(HttpClient httpClient, DataService db)
        {
            _httpClient = httpClient;
            _db = db;
            _httpClient.BaseAddress = new Uri("https://archidekt.com/api/decks/");
        }

        public async Task Download()
        {
            _startTime = DateTime.Now;
            var settings = await _db.LoadSettings();
            if (settings.ARCHPage == 0) settings.ARCHPage = 1;
            do
            {
                var response = await _httpClient.GetAsync($"cards/?orderBy=-viewCount&pageSize=50&page={settings.ARCHPage}");
                response.EnsureSuccessStatusCode();
                var data = await response.Content.ReadAsStringAsync();
                var json = JsonNode.Parse(data);
                await CreateDecks(json!["results"]!.AsArray(), settings.ARCHPage);
                settings.ARCHPage++;
                await _db.SaveSettings(settings);
            } while (true);
        }

        private async Task CreateDecks(JsonArray decks, int currentPage)
        {
            foreach (var deck in decks)
            {
                var response = await _httpClient.GetAsync($"{deck["id"]}");
                response.EnsureSuccessStatusCode();
                var data = await response.Content.ReadAsStringAsync();
                var json = JsonNode.Parse(data);

                var deckEntity = new Deck()
                {
                    Name = json["name"]!.ToString(),
                    Description = $"ID: {json["id"]}\nUserID: {json["owner"]!["id"]}\n{json["description"]}",
                    Author = new Author() { Name = json!["owner"]!["username"]!.ToString() },
                    Source = new Source() { Name = "Archidekt", Url = _httpClient.BaseAddress! },
                    Format = new Format() { Name = ParseFormat(json["deckFormat"]!.ToString().ParseToInt()) },
                    CreateDate = DateTime.Parse(json["createdAt"]!.ToString()),
                    UpdateDate = DateTime.Parse(json["updatedAt"]!.ToString()),
                    ViewCount = json["viewCount"]!.ToString().ParseToInt(),
                };

                var createdDeck = await _db.Create(deckEntity);
                var cards = await GetCards(json["cards"].AsArray(), createdDeck, DateOnly.FromDateTime(deckEntity.CreateDate));
                if (createdDeck.MissingCards != null) await _db.UpdateDeckMissingCards(createdDeck.DeckId, createdDeck.MissingCards);

                await _db.AddCardsToDeck(createdDeck, cards);

                var elapsedTime = DateTime.Now - _startTime;
                Console.WriteLine($"{elapsedTime:h'h 'm'm 's's'} | {currentPage} | {createdDeck.Name}");
            }
        }

        private async Task<List<DeckCard>> GetCards(JsonArray cardObjects, Deck deck, DateOnly date)
        {
            List<DeckCard> cards = new();

            foreach (var card in cardObjects)
            {

                var name = card["card"]!["oracleCard"]!["name"]!.ToString();
                var set = card["card"]!["edition"]!["editioncode"]!.ToString();
                var quantity = card["quantity"]!.ToString().ParseToInt();
                var isSideboard = card["categories"] != null && card["categories"]!.AsArray().Any(c => c.ToString() == "Sideboard");

                var cardEntity = await _db.GetCard(name, set);
                cardEntity ??= await _db.GetLatestCard(name, date);

                if (cardEntity != null)
                {
                    cards.Add(new DeckCard()
                    {
                        DeckId = deck.DeckId,
                        CardId = cardEntity!.CardId,
                        Quantity = quantity,
                        IsSideboard = isSideboard,
                    });
                }
                else
                {
                    if (deck.MissingCards == null) deck.MissingCards = new List<string>();
                    deck.MissingCards.Add(name);

                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Missing Card: {name} | {set}");
                    Console.ResetColor();
                }
            }

            return cards;
        }

        private string ParseFormat(int formatId)
        {
            switch (formatId)
            {
                case 1:
                    return "standard";
                case 2:
                    return "modern";
                case 3:
                    return "commander";
                case 4:
                    return "legacy";
                case 5:
                    return "vintage";
                case 6:
                    return "pauper";
                case 7:
                    return "custom";
                case 8:
                    return "frontier";
                case 9:
                    return "futureStandard";
                case 10:
                    return "pennyDreadful";
                case 11:
                case 12:
                    return "duelCommander";
                case 13:
                    return "brawl";
                case 14:
                    return "oathbreaker";
                case 15:
                    return "pioneer";
                case 16:
                    return "historic";
                case 17:
                    return "pauperCommander";
                case 18:
                    return "alchemy";
                case 19:
                    return "explorer";
                case 20:
                    return "historicBrawl";
                case 21:
                    return "gladiator";
                case 22:
                    return "premodern";
                default:
                    return string.Empty;
            }
        }
    }
}
