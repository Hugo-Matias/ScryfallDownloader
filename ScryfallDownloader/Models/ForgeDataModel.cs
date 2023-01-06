namespace ScryfallDownloader.Models
{
    public class ForgeDataModel
    {
        public Dictionary<string, List<string>> ImageSets { get; set; } = new();
        public List<ForgeEditionModel> Editions { get; set; }
    }
}
