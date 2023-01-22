using ScryfallDownloader.Data;
using ScryfallDownloader.Models;
using System.Text.Json;

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
            //List<string> formats = new() { "alchemy", "archon", "brawl", "centurion", "conquest", "duelCommander", "explorer", "gladiator", "highlanderAustralian", "highlanderCanadian", "highlanderEuropean", "historic", "historicBrawl", "legacy", "leviathan", "oldSchool", "oathbreaker", "pauper", "pauperEdh", "pennyDreadful", "pioneer", "premodern", "primordial", "tinyLeaders", "vintage", "standard", "modern", "commander", "none" };
            List<string> formats = new() { "none" };

            List<string> formatsPrecons = new() { "precons", "commanderPrecons" };

            foreach (var format in formats)
            {
                Console.WriteLine($"\n\n::: {format.ToUpper()} :::\n\n");
                var decks = await GetDecks(format, "views", true);
                //var json = JsonSerializer.Serialize<List<MoxfieldDeckModel>>(decks, new JsonSerializerOptions() { WriteIndented = true });
                //await IO.SaveTextFile($"moxfield_{format}.json", json);
            }
        }

        public async Task<List<MoxfieldDeckModel>> GetDecks(string format, string sortType, bool sortDescending)
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

        public async Task CreateMoxfieldDeckRecordsFromJson(string jsonPath)
        {
            var decks = _io.GetMoxfieldDecks(jsonPath);
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

                if (deck.Hubs != null)
                {
                    var tagList = new List<Tag>();
                    foreach (var hub in deck.Hubs)
                    {
                        var tag = new Tag() { Name = hub.Name, Description = hub.Description };
                        tagList.Add(tag);
                    }
                    deckEntity.Tags = tagList;
                }

                if (deck.Commanders != null)
                {
                    var card = await _db.GetCard(deck.Commanders[0].Name);
                    deckEntity.Commander = card;
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
