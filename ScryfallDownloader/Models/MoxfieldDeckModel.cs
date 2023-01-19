using System.Text.Json;
using System.Text.Json.Serialization;

namespace ScryfallDownloader.Models
{
    public class MoxfieldDeckModel
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("format")]
        public string Format { get; set; }
        [JsonPropertyName("publicId")]
        public string PublicId { get; set; }
        [JsonPropertyName("createdByUser")]
        [JsonConverter(typeof(MoxfieldAuthorToString))]
        public string Author { get; set; }
        [JsonPropertyName("createdAtUtc")]
        public DateTime CreatedAt { get; set; }
        [JsonPropertyName("lastUpdatedAtUtc")]
        public DateTime UpdatedAt { get; set; }
        [JsonPropertyName("hubs")]
        public List<MoxfieldDeckHubModel> Hubs { get; set; }
        [JsonPropertyName("viewCount")]
        public int Views { get; set; }
        [JsonPropertyName("likeCount")]
        public int Likes { get; set; }
        [JsonPropertyName("commentCount")]
        public int Comments { get; set; }
        [JsonPropertyName("mainboardCount")]
        public int MainboardCount { get; set; }
        public List<MoxfieldCardModel> Mainboard { get; set; }
        [JsonPropertyName("sideboardCount")]
        public int SideboardCount { get; set; }
        public List<MoxfieldCardModel> Sideboard { get; set; }
        [JsonPropertyName("maybeboardCount")]
        public int MaybeboardCount { get; set; }
        public List<MoxfieldCardModel> Maybeboard { get; set; }
        public int CommandersCount { get; set; }
        public List<MoxfieldCardModel> Commanders { get; set; }
        public int CompanionsCount { get; set; }
        public List<MoxfieldCardModel> Companions { get; set; }
        public int AttractionsCount { get; set; }
        public List<MoxfieldCardModel> Attractions { get; set; }
        public int StickersCount { get; set; }
        public List<MoxfieldCardModel> Stickers { get; set; }
    }

    public class MoxfieldDeckHubModel
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("description")]
        public string Description { get; set; }
    }

    public class MoxfieldAuthorToString : JsonConverter<string>
    {
        public override string? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            using (var jsonDoc = JsonDocument.ParseValue(ref reader))
            {
                try
                {
                    return jsonDoc.RootElement.GetProperty("userName").ToString();
                }
                catch (Exception ex)
                {
                    return jsonDoc.RootElement.ToString();
                }
            }
        }

        public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value);
        }
    }
}
