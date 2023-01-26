namespace ScryfallDownloader.Data
{
    public class CardKeyword
    {
        public int CardId { get; set; }
        public Card Card { get; set; }
        public int KeywordId { get; set; }
        public Keyword Keyword { get; set; }
    }
}
