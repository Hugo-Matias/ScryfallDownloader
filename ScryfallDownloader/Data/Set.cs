namespace ScryfallDownloader.Data
{
    public class Set
    {
        public int SetId { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public string? ForgeCode { get; set; }
        public DateOnly ReleaseDate { get; set; }
        public SetType? SetType { get; set; }
        public ICollection<Card> Cards { get; set; }
    }
}
