using System.ComponentModel.DataAnnotations.Schema;

namespace ScryfallDownloader.Data
{
    public class Card
    {
        public int CardId { get; set; }
        public string Name { get; set; }
        public string? CollectorsNumber { get; set; }
        public Rarity? Rarity { get; set; }
        public Artist? Artist { get; set; }
        public int SetId { get; set; }
        [ForeignKey(nameof(SetId))]
        public Set Set { get; set; }
        public bool IsImplemented { get; set; }
        public ICollection<DeckCard> Decks { get; set; }
        public Layout? Layout { get; set; }
        public decimal ConvertedManaCost { get; set; }
        public ICollection<CardKeyword> Keywords { get; set; }
        public ICollection<CardColor> Colors { get; set; }
        public bool IsHighres { get; set; }
        public Uri? ImageUrl { get; set; }
        public string Type { get; set; }
        public string? ManaCost { get; set; }
        public string? Power { get; set; }
        public string? Toughness { get; set; }
        public string? Loyalty { get; set; }
        public string? LifeModifier { get; set; }
        public string? HandModifier { get; set; }
        public ICollection<CardGenerateColor>? ProducedColors { get; set; }
    }
}
