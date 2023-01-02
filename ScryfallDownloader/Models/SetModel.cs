using System.Text.Json.Serialization;

namespace ScryfallDownloader.Models
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum SetType
    {
        core, expansion, masters, alchemy, masterpiece, arsenal, from_the_vault, spellbook, premium_deck, duel_deck, draft_innovation, treasure_chest, commander, planechase, archenemy, vanguard, funny, starter, box, promo, token, memorabilia
    }
    public class SetModel
    {
        public string Id { get; set; }
        public string Code { get; set; }
        [JsonPropertyName("mtgo_code")]
        public string? MtgoCode { get; set; }
        [JsonPropertyName("tcgplayer_id")]
        public int? TcgplayerId { get; set; }
        public string Name { get; set; }
        [JsonPropertyName("set_type")]
        public SetType SetType { get; set; }
        [JsonPropertyName("released_at")]
        public DateOnly? ReleasedAt { get; set; }
        [JsonPropertyName("block_code")]
        public string? BlockCode { get; set; }
        public string? Block { get; set; }
        [JsonPropertyName("parent_set_code")]
        public string? ParentSetCode { get; set; }
        [JsonPropertyName("card_count")]
        public int CardCount { get; set; }
        [JsonPropertyName("printed_size")]
        public int? PrintedSize { get; set; }
        public bool Digital { get; set; }
        [JsonPropertyName("foil_only")]
        public bool FoilOnly { get; set; }
        [JsonPropertyName("nonfoil_only")]
        public bool NonFoilOnly { get; set; }
        [JsonPropertyName("scryfall_uri")]
        public Uri ScryfallUri { get; set; }
        public Uri Uri { get; set; }
        [JsonPropertyName("icon_svg_uri")]
        public Uri IconSvgUri { get; set; }
        [JsonPropertyName("search_uri")]
        public Uri SearchUri { get; set; }
    }
}
