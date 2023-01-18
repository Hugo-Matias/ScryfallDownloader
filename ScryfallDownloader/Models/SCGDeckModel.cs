namespace ScryfallDownloader.Models
{
    public class SCGDeckModel
    {
        public string Name { get; set; }
        public Uri Link { get; set; }
        public int Finish { get; set; }
        public string Player { get; set; }
        public string? Event { get; set; }
        public string Format { get; set; }
        public DateOnly Date { get; set; }
        public string? Location { get; set; }
        public string? Decklist { get; set; }
    }
}
