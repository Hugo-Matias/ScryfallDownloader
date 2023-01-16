namespace ScryfallDownloader.Models
{
    public class DeckCardModel
    {
        public string Name { get; set; }
        public int Quantity { get; set; }
        public int Variations { get; set; }
        public string? Set { get; set; }
        public string? CollectorNumber { get; set; }
    }
}
