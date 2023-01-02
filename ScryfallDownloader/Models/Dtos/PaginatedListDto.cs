using System.Text.Json.Serialization;

namespace ScryfallDownloader.Models.Dtos
{
    public class PaginatedListDto
    {
        [JsonPropertyName("has_more")]
        public bool HasMore { get; set; }
        [JsonPropertyName("next_page")]
        public Uri? NextPage { get; set; }
        [JsonPropertyName("total_cards")]
        public int? TotalCards { get; set; }
        public List<string> Warnings { get; set; }
    }
}
