namespace ScryfallDownloader.Models
{
    public class BaseDeckModel
    {
        public string Name { get; set; }
        public Uri Link { get; set; }
        public string Description { get; set; }
        public int Finish { get; set; }
        public string Player { get; set; }
        public string? Event { get; set; }
        public string Format { get; set; }
        public DateTime Date { get; set; }
        public string? Location { get; set; }
        public List<BaseCardModel>? Cards { get; set; }
    }
}
