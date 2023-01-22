using ScryfallDownloader.Data;
using System.Globalization;
using System.Text.Json.Nodes;

namespace ScryfallDownloader.Services
{
    public class EdhrecDownloaderService
    {
        private readonly HttpClient _httpClient;
        private readonly DataService _db;

        public EdhrecDownloaderService(HttpClient httpClient, DataService db)
        {
            _httpClient = httpClient;
            _db = db;
            _httpClient.BaseAddress = new Uri("https://json.edhrec.com/pages/");
        }

        public async Task Download()
        {
            if (!await _db.CheckEdhrecCommandersPopulated())
            {
                var commanders = await GetCommanders();
                foreach (var commander in commanders) { await _db.Create(commander); }
            }

            var commanderEntities = await _db.GetEdhrecCommanders();
            foreach (var entity in commanderEntities) { await CreateDecks(entity.Link); }
        }

        private async Task<List<EdhrecCommander>> GetCommanders()
        {
            var jsonString = await _httpClient.GetStringAsync("commanders/year.json");
            var jsonObject = JsonNode.Parse(jsonString);
            var commanders = CreateCommanderEntities(jsonObject!["container"]!["json_dict"]!["cardlists"]![0]!["cardviews"]!.AsArray());

            var page = 1;
            HttpResponseMessage response;
            do
            {
                Console.WriteLine($"========================Page: {page}");
                response = await _httpClient.GetAsync($"commanders/year-past2years-{page}.json");
                if (!response.IsSuccessStatusCode) break;
                jsonString = await response.Content.ReadAsStringAsync();
                jsonObject = JsonNode.Parse(jsonString);
                commanders.AddRange(CreateCommanderEntities(jsonObject!["cardviews"]!.AsArray()));
                page++;
            } while (true);

            return commanders;
        }

        private List<EdhrecCommander> CreateCommanderEntities(JsonArray cardviews)
        {
            List<EdhrecCommander> commanders = new();
            foreach (var cardview in cardviews)
            {
                var name = cardview!["name"].ToString();
                var link = cardview!["sanitized"].ToString();
                var deckCount = (int)cardview!["num_decks"];

                commanders.Add(new EdhrecCommander() { Name = name, Link = link, DeckCount = deckCount });

                Console.WriteLine($"{name} | {link} | {deckCount}");
            }
            return commanders;
        }

        private async Task CreateDecks(string commander)
        {
            var settings = await _db.LoadSettings();

            if (commander != settings.EDHCommander) return;

            var response = await _httpClient.GetStringAsync($"decks/{commander}.json");
            var json = JsonNode.Parse(response);

            List<string> deckHashes = new();
            foreach (var node in json!["table"]!.AsArray()) { deckHashes.Add(node["urlhash"].ToString()); }

            var deckIndex = 0;
            foreach (var hash in deckHashes)
            {
                deckIndex++;
                if (deckIndex < settings.EDHDeck) continue;

                response = await _httpClient.GetStringAsync($"deckpreview-temp/{hash}.json");
                json = JsonNode.Parse(response);

                var name = $"{json!["commanders"]![0]} - {hash}";
                var date = DateTime.ParseExact(json!["deckage"]!.ToString(), "yyyy-MM-dd", CultureInfo.InvariantCulture);

                if (await _db.CheckDeckExists(name, "EDHREC", "EDHREC", date)) continue;

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"{deckIndex}/{deckHashes.Count}: {hash}");
                Console.ResetColor();

                Deck deck = new()
                {
                    Name = name,
                    Description = $"Salt: {json!["salt"]}\n{json!["panels"]!["deckinfo"]!["deck_preview"]!["second_row"]!}",
                    Author = new Author() { Name = "EDHREC" },
                    Source = new Source() { Name = "EDHREC", Url = new Uri("https://edhrec.com/decks/") },
                    Format = new Format() { Name = "commander" },
                    CreateDate = date,
                    UpdateDate = DateTime.ParseExact(json!["savedate"]!.ToString(), "yyyy-MM-dd", CultureInfo.InvariantCulture),
                    Tags = new List<Tag>()
                };
                if (json["theme"] != null) deck.Tags.Add(new Tag() { Name = json!["theme"]!.ToString() });
                if (json["tribe"] != null) deck.Tags.Add(new Tag() { Name = json!["tribe"]!.ToString() });

                var createdDeck = await _db.Create(deck);

                if (json["commanders"] != null)
                {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine(json["commanders"][0]);
                    var commanderEntity = await _db.GetLatestCard(json["commanders"][0].ToString(), DateOnly.FromDateTime(createdDeck.CreateDate));

                    if (json["commanders"]!.AsArray().Count > 1)
                    {
                        Console.WriteLine(json["commanders"][1]);
                        var commander2Entity = await _db.GetLatestCard(json["commanders"][1].ToString(), DateOnly.FromDateTime(createdDeck.CreateDate));
                        await _db.AddCommandersToDeck(createdDeck.DeckId, commanderEntity.CardId, commander2Entity.CardId);
                    }
                    else await _db.AddCommandersToDeck(createdDeck.DeckId, commanderEntity.CardId);
                    Console.ResetColor();
                }

                List<DeckCard> cards = new();
                List<string> missingCards = null;
                foreach (var card in json["cards"].AsArray())
                {
                    if (string.IsNullOrWhiteSpace(card.ToString())) continue;
                    var cardEntity = await _db.GetLatestCard(card.ToString(), DateOnly.FromDateTime(deck.CreateDate), false);
                    if (cardEntity != null)
                    {
                        if (cards.Any(c => c.CardId.Equals(cardEntity.CardId)))
                        {
                            var deckCard = cards.FirstOrDefault(c => c.CardId.Equals(cardEntity.CardId));
                            deckCard.Quantity += 1;
                        }
                        else
                        {
                            cards.Add(new DeckCard()
                            {
                                Deck = createdDeck,
                                DeckId = createdDeck.DeckId,
                                Card = cardEntity,
                                CardId = cardEntity.CardId,
                                Quantity = 1,
                                IsSideboard = false
                            });
                        }
                        //await _db.AddCardToDeck(createdDeck.DeckId, cardEntity.CardId, 1, false);
                        Console.WriteLine($"{deckIndex}||{card}");
                    }
                    else
                    {
                        if (missingCards == null) missingCards = new List<string>();
                        missingCards.Add(card.ToString());
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"{deckIndex}||{card}");
                        Console.ResetColor();
                    }
                }
                if (missingCards != null && missingCards.Count > 0) { await _db.UpdateDeckMissingCards(createdDeck.DeckId, missingCards); }
                settings.EDHDeck++;
                await _db.SaveSettings(settings);
            }
            settings.EDHCommander = commander;
            settings.EDHDeck = 1;
            await _db.SaveSettings(settings);
        }
    }
}
