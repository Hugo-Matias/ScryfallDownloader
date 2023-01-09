namespace ScryfallDownloader.Services
{
    public class CardDownloaderService
    {
        private readonly HttpClient _httpClient;

        public CardDownloaderService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri("https://cards.scryfall.io/");
        }

        public Task<byte[]> DownloadImage(string url) => _httpClient.GetByteArrayAsync(url);
    }
}
