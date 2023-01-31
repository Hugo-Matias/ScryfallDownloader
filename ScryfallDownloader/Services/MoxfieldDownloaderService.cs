using ScryfallDownloader.Data;
using ScryfallDownloader.Extensions;
using ScryfallDownloader.Models;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace ScryfallDownloader.Services
{
    public class MoxfieldDownloaderService
    {
        private readonly HttpClient _httpClient;
        private readonly DataService _db;
        private readonly IOService _io;

        public MoxfieldDownloaderService(HttpClient httpClient, DataService db, IOService io)
        {
            _httpClient = httpClient;
            _db = db;
            _io = io;
            _httpClient.BaseAddress = new Uri("https://api2.moxfield.com/v2/decks/");
        }

        public async Task Download()
        {
            var settings = await _db.LoadSettings();
            while (true)
            {
                var cancel = await CreateDecks(settings.MOXPage, "views", true);
                if (cancel) break;

                settings.MOXPage++;
                await _db.SaveSettings(settings);
            }
            Console.WriteLine("\n\nDeck Scraping Has Finished!");
        }

        private async Task<bool> CreateDecks(int page, string sortType, bool sortDescending)
        {
            var pageSize = 100;
            var sortDirection = sortDescending ? "Descending" : "Ascending";
            var query = $"search?pageNumber={page}&pageSize={pageSize}&sortType={sortType}&sortDirection={sortDirection}";

            var response = await _httpClient.GetAsync(query);
            response.EnsureSuccessStatusCode();
            var data = await response.Content.ReadAsStringAsync();
            var json = JsonNode.Parse(data);

            var totalPages = json!["totalPages"]!.ToString();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"Page: {page} / {totalPages}");
            Console.ResetColor();

            var moxDecks = JsonSerializer.Deserialize<List<MoxfieldDeckModel>>(json["data"]);

            var deckIndex = 0;
            foreach (var moxDeck in moxDecks)
            {
                deckIndex++;
                Deck deck = new();

                deck.Name = moxDeck.Name;
                deck.Description = $"PublicID: {moxDeck.PublicId}";
                deck.Author = new Author() { Name = moxDeck.Author };
                deck.Source = new Source() { Name = "Moxfield", Url = new Uri("https://www.moxfield.com/") };
                deck.Format = new Format() { Name = moxDeck.Format };
                deck.CreateDate = moxDeck.CreatedAt;
                deck.UpdateDate = moxDeck.UpdatedAt;
                deck.ViewCount = moxDeck.Views;
                deck.LikeCount = moxDeck.Likes;
                deck.CommentCount = moxDeck.Comments;

                if (await _db.CheckDeckExists(deck.Name, deck.Author.Name, deck.Source.Name, deck.CreateDate)) continue;

                response = await _httpClient.GetAsync($"all/{moxDeck.PublicId}");
                response.EnsureSuccessStatusCode();
                data = await response.Content.ReadAsStringAsync();
                json = JsonNode.Parse(data);

                if (json["hubs"] != null && json["hubs"].AsArray().Count > 0)
                {
                    deck.Tags = new List<DeckTag>();
                    foreach (var hub in json["hubs"].AsArray())
                    {
                        deck.Tags.Add(new DeckTag()
                        {
                            Tag = new Tag()
                            {
                                Name = hub["name"].ToString(),
                                Description = hub["description"].ToString()
                            }
                        });
                    }
                }

                if (json["commandersCount"] != null && (int)json["commandersCount"] > 0)
                {
                    var commanders = json["commanders"].AsObject();
                    var commander = commanders.ElementAt(0).Value["card"];
                    deck.Commander = new()
                    {
                        Name = commander["name"].ToString().ParseSplitCardname(),
                        Set = new() { Code = commander["set"].ToString() }
                    };

                    if ((int)json["commandersCount"] > 1)
                    {
                        commander = commanders.ElementAt(1).Value["card"];
                        deck.Commander2 = new()
                        {
                            Name = commander["name"].ToString().ParseSplitCardname(),
                            Set = new() { Code = commander["set"].ToString() }
                        };
                    }
                }

                var createdDeck = await _db.Create(deck);
                List<DeckCard> cardEntities = new();

                if ((int)json["mainboardCount"] > 0)
                {
                    var cards = json["mainboard"].AsObject().ToList();
                    var mainboard = await CreateDeckCardList(cards, createdDeck, false);
                    cardEntities = mainboard;
                }

                if ((int)json["sideboardCount"] > 0)
                {
                    var cards = json["sideboard"].AsObject().ToList();
                    var sideboard = await CreateDeckCardList(cards, createdDeck, true);
                    if (cardEntities != null)
                    {
                        cardEntities.AddRange(sideboard);
                    }
                    else cardEntities = sideboard;
                }

                if (deck.MissingCards != null) { await _db.UpdateDeckMissingCards(createdDeck.DeckId, deck.MissingCards); }

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"{deckIndex} | {createdDeck.DeckId}: {createdDeck.Name}");
                Console.ResetColor();

                await _db.AddCardsToDeck(createdDeck, cardEntities);
            }
            return false;
        }

        private async Task<List<DeckCard>> CreateDeckCardList(List<KeyValuePair<string, JsonNode>> cards, Deck deck, bool isSideboard)
        {
            List<DeckCard> cardEntities = new();
            foreach (var obj in cards)
            {
                var name = obj.Value["card"]["name"].ToString();
                var set = obj.Value["card"]["set"].ToString();
                var quantity = (int)obj.Value["quantity"];

                var cardEntity = await _db.GetCard(name, set);
                cardEntity ??= await _db.GetLatestCard(name, DateOnly.FromDateTime(deck.CreateDate));

                if (cardEntity != null)
                {

                    var card = new DeckCard()
                    {
                        DeckId = deck.DeckId,
                        CardId = cardEntity.CardId,
                        Quantity = quantity,
                        IsSideboard = isSideboard
                    };
                    if (cardEntities.Any(c => c.CardId == card.CardId && c.DeckId == card.DeckId && c.IsSideboard == card.IsSideboard))
                    {
                        cardEntities.FirstOrDefault(c => c.CardId == card.CardId && c.IsSideboard == card.IsSideboard).Quantity += card.Quantity;
                    }
                    else cardEntities.Add(card);
                }
                else
                {
                    if (deck.MissingCards == null) deck.MissingCards = new List<string>();
                    deck.MissingCards.Add(name);
                }
            }
            return cardEntities;
        }

        public async Task DownloadFormats()
        {
            //List<string> formats = new() { "alchemy", "archon", "brawl", "centurion", "conquest", "duelCommander", "explorer", "gladiator", "highlanderAustralian", "highlanderCanadian", "highlanderEuropean", "historic", "historicBrawl", "legacy", "leviathan", "oldSchool", "oathbreaker", "pauper", "pauperEdh", "pennyDreadful", "pioneer", "premodern", "primordial", "tinyLeaders", "vintage", "standard", "modern", "commander", "none" };
            List<string> formats = new() { "none" };

            List<string> formatsPrecons = new() { "precons", "commanderPrecons" };

            foreach (var format in formats)
            {
                Console.WriteLine($"\n\n::: {format.ToUpper()} :::\n\n");
                var decks = await GetDecks(format, "views", true);
                await CreateDecks(decks);
                //var json = JsonSerializer.Serialize<List<MoxfieldDeckModel>>(decks, new JsonSerializerOptions() { WriteIndented = true });
                //await IO.SaveTextFile($"moxfield_{format}.json", json);
            }
        }

        private async Task<List<MoxfieldDeckModel>> GetDecks(string format, string sortType, bool sortDescending)
        {
            List<MoxfieldDeckModel> decks = new();

            var pageSize = 100;
            var pageNumber = 1;
            var totalPages = int.MaxValue;
            var sortDirection = sortDescending ? "Descending" : "Ascending";
            var cancel = false;
            var cancelMinimum = 30;
            //var format = "precons";

            var cancelableFormats = new List<string>() { "commander", "modern", "standard" };

            do
            {
                var query = $"search?pageNumber={pageNumber}&pageSize={pageSize}&sortType={sortType}&sortDirection={sortDirection}&fmt={format}";
                var response = await _httpClient.GetStringAsync(query);
                var json = JsonDocument.Parse(response).RootElement;
                var data = json.GetProperty("data");

                totalPages = int.Parse(json.GetProperty("totalPages").ToString());
                Console.WriteLine($"Page: {pageNumber} / {totalPages}");

                var pageDecks = JsonSerializer.Deserialize<List<MoxfieldDeckModel>>(data);
                decks.AddRange(pageDecks);

                if (cancelableFormats.Contains(format)) cancel = CheckDeckViewsLower(decks, cancelMinimum);

                pageNumber++;
                //} while (pageNumber <= 1);
            } while (pageNumber <= totalPages && !cancel);

            decks.OrderByDescending(d => d.CreatedAt);

            return await PopulateDecklist(decks);
        }

        private async Task<List<MoxfieldDeckModel>> PopulateDecklist(List<MoxfieldDeckModel> decks)
        {
            var deckIndex = 0;

            foreach (var deck in decks)
            {
                Console.WriteLine($"{deckIndex + 1}/{decks.Count}: {deck.Name}");
                deckIndex++;

                var query = $"all/{deck.PublicId}";
                try
                {
                    var response = await _httpClient.GetStringAsync(query);
                    var json = JsonDocument.Parse(response).RootElement;

                    deck.CommandersCount = int.Parse(json.GetProperty("commandersCount").ToString());
                    deck.CompanionsCount = int.Parse(json.GetProperty("companionsCount").ToString());
                    deck.StickersCount = int.Parse(json.GetProperty("stickersCount").ToString());

                    if (deck.MainboardCount > 0) deck.Mainboard = ParseCardSection(json.GetProperty("mainboard"));
                    if (deck.SideboardCount > 0) deck.Sideboard = ParseCardSection(json.GetProperty("sideboard"));
                    if (deck.MaybeboardCount > 0) deck.Maybeboard = ParseCardSection(json.GetProperty("maybeboard"));
                    if (deck.CommandersCount > 0) deck.Commanders = ParseCardSection(json.GetProperty("commanders"));
                    if (deck.CompanionsCount > 0) deck.Companions = ParseCardSection(json.GetProperty("companions"));
                    if (deck.AttractionsCount > 0) deck.Attractions = ParseCardSection(json.GetProperty("attractions"));
                    if (deck.StickersCount > 0) deck.Stickers = ParseCardSection(json.GetProperty("stickers"));

                    //if (deck.CompanionsCount > 0) Console.WriteLine("----Companions: " + deck.PublicId);
                    //if (deck.AttractionsCount > 0) Console.WriteLine("----Attractions: " + deck.PublicId);
                    //if (deck.StickersCount > 0) Console.WriteLine("-----Stickers: " + deck.PublicId);
                }
                catch { Console.WriteLine($"\nCouldn't get deck: {deck.Name}\n{deck.PublicId}\n"); continue; }
            }

            return decks;
        }

        private async Task CreateDecks(List<MoxfieldDeckModel> decks)
        {
            var deckIndex = 0;
            foreach (var deck in decks)
            {
                deckIndex++;
                if (await _db.CheckDeckExists(deck.Name, deck.CreatedAt)) continue;
                Console.WriteLine($"\n\nCreating Deck {deckIndex}/{decks.Count}: {deck.Name}\n");

                var deckEntity = new Deck();
                deckEntity.Name = deck.Name;
                deckEntity.Description = $"PublicID: {deck.PublicId}";
                deckEntity.CreateDate = deck.CreatedAt;
                deckEntity.UpdateDate = deck.UpdatedAt;
                deckEntity.ViewCount = deck.Views;
                deckEntity.LikeCount = deck.Likes;
                deckEntity.CommentCount = deck.Comments;
                deckEntity.Author = new Author() { Name = deck.Author };
                deckEntity.Format = new Format() { Name = deck.Format.ToLower() };
                deckEntity.Source = new Source() { Name = "Moxfield", Url = new Uri("https://www.moxfield.com/") };

                //if (deck.Hubs != null)
                //{
                //    var tagList = new List<Tag>();
                //    foreach (var hub in deck.Hubs)
                //    {
                //        var tag = new Tag() { Name = hub.Name, Description = hub.Description };
                //        tagList.Add(tag);
                //    }
                //    deckEntity.Tags = tagList;
                //}

                if (deck.Commanders != null)
                {
                    deckEntity.Commander = await _db.GetLatestCard(deck.Commanders[0].Name, DateOnly.FromDateTime(deckEntity.CreateDate));
                    if (deck.Commanders.Count > 1)
                        deckEntity.Commander2 = await _db.GetLatestCard(deck.Commanders[1].Name, DateOnly.FromDateTime(deckEntity.CreateDate));
                }

                var createdDeckEntity = await _db.Create(deckEntity);

                var cardIndex = 0;
                Console.WriteLine("\nMainboard:");

                if (deck.Mainboard != null)
                {
                    foreach (var card in deck.Mainboard)
                    {
                        cardIndex++;
                        Console.WriteLine($"{deckIndex}||{cardIndex}/{deck.Mainboard.Count}: {card.Name}");

                        var cardEntity = await _db.GetCard(card.Name, card.Set);

                        if (cardEntity != null)
                            await _db.AddCardToDeck(createdDeckEntity.DeckId, cardEntity.CardId, card.Quantity, false);
                        else
                            Console.WriteLine($"----CARD NOT FOUND: {card.Name} | {card.Set}");
                    }
                }

                if (deck.Sideboard != null)
                {
                    cardIndex = 0;
                    Console.WriteLine("\nSideboard:");

                    foreach (var card in deck.Sideboard)
                    {
                        cardIndex++;
                        Console.WriteLine($"{deckIndex}||{cardIndex}/{deck.Sideboard.Count}: {card.Name}");

                        var cardEntity = await _db.GetCard(card.Name, card.Set);

                        if (cardEntity != null)
                            await _db.AddCardToDeck(createdDeckEntity.DeckId, cardEntity.CardId, card.Quantity, true);
                        else
                            Console.WriteLine($"----CARD NOT FOUND: {card.Name} | {card.Set}");
                    }
                }
            }
        }

        private async Task CreateDecksFromJson(string jsonPath)
        {
            var decks = _io.GetMoxfieldDecks(jsonPath);
            await CreateDecks(decks);
        }

        private List<MoxfieldCardModel> ParseCardSection(JsonElement json)
        {
            List<MoxfieldCardModel> cards = new();

            foreach (var cardElement in json.EnumerateObject())
            {
                MoxfieldCardModel card = new();

                var cardJson = JsonDocument.Parse(cardElement.Value.ToString()).RootElement;

                card.Name = cardJson.GetProperty("card").GetProperty("name").ToString();
                card.Set = cardJson.GetProperty("card").GetProperty("set").ToString().ToLower();
                card.SetName = cardJson.GetProperty("card").GetProperty("set_name").ToString();
                card.CollectorsNumber = cardJson.GetProperty("card").GetProperty("cn").ToString();
                card.Quantity = int.Parse(cardJson.GetProperty("quantity").ToString());

                cards.Add(card);
            }

            return cards;
        }

        private bool CheckDeckViewsLower(List<MoxfieldDeckModel> decks, int viewsMinimum) => decks.Any(d => d.Views < viewsMinimum);
    }
}
