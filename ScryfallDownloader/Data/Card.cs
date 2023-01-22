namespace ScryfallDownloader.Data
{
    public class Card
    {
        public int CardId { get; set; }
        public string Name { get; set; }
        public string? CollectorsNumber { get; set; }
        public Rarity? Rarity { get; set; }
        public Artist? Artist { get; set; }
        public Set Set { get; set; }
        public bool IsImplemented { get; set; }
        public ICollection<DeckCard> Decks { get; set; }
    }
}
