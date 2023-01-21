using HtmlAgilityPack;
using ScryfallDownloader.Extensions;
using ScryfallDownloader.Models;
using System.Net;

namespace ScryfallDownloader.Services
{
    public class StarCityGamesScraper
    {
        private readonly HttpClient _httpClient;

        public StarCityGamesScraper(HttpClient httpClient)
        {
            _httpClient = httpClient;
            //_httpClient.BaseAddress = new Uri("https://old.starcitygames.com");
        }

        /// <summary>
        /// Fetches data from the specified SCG url and serializes it into SCG models.
        /// </summary>
        /// <param name="url"></param>
        /// <param name="currentPage"></param>
        /// <returns>Tuple of (string nextPageUrl, List<SCGDeckModel> parsedDeckModels)</returns>
        public async Task<(string, List<SCGDeckModel>)?> GetDecks(string url, int currentPage)
        {
            if (string.IsNullOrWhiteSpace(url)) return null;

            List<SCGDeckModel> decks = new();

            var response = await _httpClient.GetStringAsync(url);

            var doc = new HtmlDocument();
            doc.LoadHtml(response);

            var tableRows = doc.DocumentNode.SelectNodes("//*[@id='content']/table/tr");

            var nextPageElement = tableRows.Where(n => n.FirstChild.InnerText.Contains("- Next")).ToList();
            string nextPageLink = string.Empty;
            if (nextPageElement.Count > 0)
            {
                nextPageLink = nextPageElement[0].FirstChild.ChildNodes.Where(c => c.FirstChild.InnerText.ToLower().Contains("next")).ToList()[0].GetAttributeValue("href", "");
            }

            var deckElements = tableRows.Where(n => n.ChildNodes.Any(c => c.GetAttributeValue("class", "").Contains("deckdbbody")));

            var deckIndex = 0;
            var decksTotal = deckElements.Count();
            foreach (var node in deckElements)
            {
                deckIndex++;
                if (node.ChildNodes.Count() != 8) { continue; }

                var name = node.FirstChild.InnerText;
                var link = new Uri(node.FirstChild.FirstChild.GetAttributeValue("href", ""));
                var finish = node.ChildNodes[1].InnerText.ParseToInt();
                var player = node.ChildNodes[3].InnerText;
                var eventName = node.ChildNodes[4].InnerText;
                var format = node.ChildNodes[5].InnerText;
                var date = DateTime.Parse(node.ChildNodes[6].FirstChild.InnerText);
                var location = node.ChildNodes[7].InnerText;
                var description = $"Code: {link.Segments.LastOrDefault()}\r\nPlace: {finish}\r\nEvent: {eventName}\r\nLocation: {location}";

                //Console.WriteLine("|" + link.Segments.LastOrDefault() + "|");
                Console.WriteLine($"Page:{currentPage}||Deck:{deckIndex}/{decksTotal}||{name} - {link.Segments.LastOrDefault()}");

                var decklistString = await GetDecklist(link.Segments.LastOrDefault());

                if (string.IsNullOrWhiteSpace(decklistString)) { Console.WriteLine($"EMPTY DECKLIST: {name} - {link.AbsolutePath}"); continue; }
                var deck = ParseDecklist(decklistString);

                decks.Add(new SCGDeckModel() { Name = name, Link = link, Description = description, Finish = finish, Player = player, Event = eventName, Format = format, Date = date, Location = location, Cards = deck });
            }

            return (nextPageLink, decks);
        }

        private async Task<string> GetDecklist(string code)
        {
            var response = await _httpClient.GetAsync($"https://old.starcitygames.com/decks/download_deck?DeckID={code}&Mode=modo");

            if (response.StatusCode == HttpStatusCode.Redirect)
            {
                return await _httpClient.GetStringAsync(response.Headers.Location.AbsoluteUri);
            }
            else
            {
                Console.WriteLine($"NO REDIRECT FOR: {code}");
                return string.Empty;
            }
        }

        private List<SCGCardModel> ParseDecklist(string decklist)
        {
            List<SCGCardModel> deck = new();
            var section = "mainboard";

            foreach (var line in decklist.SplitToLines())
            {
                var groups = line.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);

                if (groups.Length < 2)
                {
                    section = line;
                    continue;
                }

                var quantity = groups[0].ParseToInt();
                var name = groups[1];
                deck.Add(new SCGCardModel { Name = name, Quantity = quantity, IsSideboard = section != "mainboard" });
            }

            return deck;
        }
    }
}
