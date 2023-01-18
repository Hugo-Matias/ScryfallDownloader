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

        public async Task<string> GetDecks(List<SCGDeckModel> decks, string url)
        {
            if (string.IsNullOrWhiteSpace(url)) return "";

            var response = await GetPage(url);

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

            foreach (var node in deckElements)
            {
                if (node.ChildNodes.Count() != 8) { continue; }

                var name = node.FirstChild.InnerText;
                var link = new Uri(node.FirstChild.FirstChild.GetAttributeValue("href", ""));
                var finish = ParsingHelper.ParseToInt(node.ChildNodes[1].InnerText);
                var player = node.ChildNodes[3].InnerText;
                var eventName = node.ChildNodes[4].InnerText;
                var format = node.ChildNodes[5].InnerText;
                var date = DateOnly.Parse(node.ChildNodes[6].FirstChild.InnerText);
                var location = node.ChildNodes[7].InnerText;

                Console.WriteLine("|" + link.Segments.LastOrDefault() + "|");

                var decklist = await GetDecklist(link.Segments.LastOrDefault());

                if (string.IsNullOrWhiteSpace(decklist)) { Console.WriteLine($"EMPTY DECKLIST: {name} - {link.AbsolutePath}"); ; }

                decks.Add(new SCGDeckModel() { Name = name, Link = link, Finish = finish, Player = player, Event = eventName, Format = format, Date = date, Location = location });
            }

            return nextPageLink;
        }

        private async Task<string> GetDecklist(string code)
        {
            var response = await _httpClient.GetAsync($"https://old.starcitygames.com/decks/download_deck?DeckID={code}&Mode=modo");

            if (response.StatusCode == HttpStatusCode.Redirect)
            {
                return await _httpClient.GetStringAsync(response.Headers.Location.AbsoluteUri);
            }
            else
                throw new Exception($"NO REDIRECT FOR: {code}");
        }

        private async Task<string> GetPage(string url) => await _httpClient.GetStringAsync(url);
    }
}
