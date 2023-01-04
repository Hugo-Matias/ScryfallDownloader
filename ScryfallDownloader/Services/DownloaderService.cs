namespace ScryfallDownloader.Services
{
    public class DownloaderService
    {
        private readonly HttpClient _httpClient;

        public DownloaderService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri("https://cards.scryfall.io/");
        }

        public Task<byte[]> DownloadImage(string url) => _httpClient.GetByteArrayAsync(url);
    }
}
