namespace ScryfallDownloader.Services
{
    public class DataDownloaderService
    {
        private readonly HttpClient _httpClient;
        private readonly IOService _io;

        public DataDownloaderService(HttpClient httpClient, IOService io)
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri("https://data.scryfall.io/");
            _io = io;
        }

        public async Task DownloadJsonData(string uri)
        {
            var data = await _httpClient.GetByteArrayAsync(uri);
            await _io.SaveFile("cards.json", data);
        }
    }
}
