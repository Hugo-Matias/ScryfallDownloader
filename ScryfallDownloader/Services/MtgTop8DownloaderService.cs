using HtmlAgilityPack;
using ScryfallDownloader.Extensions;
using ScryfallDownloader.Models;
using System.Globalization;
using System.Text.RegularExpressions;

namespace ScryfallDownloader.Services
{
    public partial class MtgTop8DownloaderService
    {
        private readonly HttpClient _httpClient;
        private readonly DataService _db;
        private readonly Regex _deckCodeRe;
        private readonly Regex _eventCodeRe;

        public MtgTop8DownloaderService(HttpClient httpClient, DataService db)
        {
            _httpClient = httpClient;
            _db = db;
            _httpClient.BaseAddress = new Uri("https://mtgtop8.com/");
            _deckCodeRe = new Regex(@"d=(\d+)", RegexOptions.Compiled);
            _eventCodeRe = new Regex(@"e=(\d+)", RegexOptions.Compiled);
        }

        public async Task Download()
        {
            var settings = await _db.LoadSettings();
            do
            {
                var decks = await GetDecks(settings.MT8Page);
                if (decks == null) break;

                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"=========================================================Page: {settings.MT8Page}");
                Console.ResetColor();
                await _db.CreateDeckEntities(decks, "MTGTop8", "https://mtgtop8.com/");

                settings.MT8Page++;
                await _db.SaveSettings(settings);
            } while (true);

            Console.WriteLine("\n\nDeck Scraping Has Finished!");
        }

        public async Task<List<BaseDeckModel>?> GetDecks(int currentPage)
        {
            var payload = new Dictionary<string, string>() { { "current_page", currentPage.ToString() } };
            var content = new FormUrlEncodedContent(payload);
            var response = await _httpClient.PostAsync("search", content);
            var responseString = await response.Content.ReadAsStringAsync();

            var doc = new HtmlDocument();
            doc.LoadHtml(responseString);

            var tableRows = doc.DocumentNode.SelectNodes("//table[contains(@class, 'Stable')]/tr/td[contains(@class, 'S12')]/..");
            if (tableRows == null) return null;

            var decks = new List<BaseDeckModel>();
            foreach (var row in tableRows)
            {
                if (row.ChildNodes.Count < 17)
                {
                    Console.WriteLine(row.ChildNodes.Where(n => n.Name == "td").ToList()[1].InnerHtml);
                    continue;
                }

                var cols = row.ChildNodes.Where(r => r.Name == "td").ToList();

                // First column (cols[0]) is a checkbox and should be ignored
                var name = cols[1].FirstChild.InnerText;
                var link = cols[1].FirstChild.GetAttributeValue("href", "");
                var linkUri = new Uri(_httpClient.BaseAddress + link);
                var deckCode = _deckCodeRe.Match(link).Groups[1].Value;
                var eventCode = _eventCodeRe.Match(link).Groups[1].Value;
                var author = cols[2].InnerText;
                var format = cols[3].InnerText.ParseDeckFormat();
                var gameEvent = cols[4].InnerText;
                var rank = cols[6].InnerText;
                var date = DateTime.ParseExact(cols[7].InnerText, "dd/MM/yy", CultureInfo.InvariantCulture);

                var deck = new BaseDeckModel() { Name = name, Link = linkUri, Player = author, Event = gameEvent, Format = format, Date = date };
                deck.Description = $"Deck Code: {deckCode}\nEvent Code: {eventCode}\nRank: {rank}\nEvent: {gameEvent}";
                deck.Cards = await GetCards(deckCode);

                decks.Add(deck);
            }
            return decks;
        }

        private async Task<List<BaseCardModel>> GetCards(string code)
        {
            var response = await _httpClient.GetStringAsync($"mtgo?d={code}");

            List<BaseCardModel> deck = new();
            var section = "mainboard";

            foreach (var line in response.SplitToLines())
            {
                var groups = line.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);

                if (groups.Length < 2)
                {
                    section = line;
                    continue;
                }

                var quantity = groups[0].ParseToInt();
                var name = groups[1];
                // Flip and double faced cards name separator is a single slash for MT8 but our database's convention is a double slash,
                // To find the proper card we need to parse the name accordingly.
                name = name.Replace(" / ", " // ");
                deck.Add(new BaseCardModel { Name = name, Quantity = quantity, IsSideboard = section != "mainboard" });
            }

            return deck;
        }
    }
}
