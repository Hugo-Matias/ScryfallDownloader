namespace ScryfallDownloader.Models
{
    public class ForgeEditionModel
    {
        public string Code { get; set; }
        public string? ScryfallCode { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public DateOnly Date { get; set; }
        public string? Code2 { get; set; }
        public string? Alias { get; set; }
        public List<ForgeCardModel>? Cards { get; set; }
        public List<ForgeCardModel>? Tokens { get; set; }
        public Dictionary<string, List<string>>? Other { get; set; }
    }
}
