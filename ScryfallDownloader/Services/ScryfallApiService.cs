using ScryfallDownloader.Models;
using ScryfallDownloader.Models.Dtos;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ScryfallDownloader.Services
{
    public class ScryfallApiService
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _options;

        public ScryfallApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri("https://api.scryfall.com/");
            _httpClient.Timeout = TimeSpan.FromMinutes(2);

            _options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull };
        }

        public async Task<List<SetModel>> GetSets()
        {
            var response = await _httpClient.GetAsync("sets");
            var content = await response.Content.ReadAsStringAsync();

            response.EnsureSuccessStatusCode();

            var sets = JsonSerializer.Deserialize<SetListDto>(content, _options);
            return sets.Data;
        }
    }
}
