using ScryfallApi.Client.Models;

namespace ScryfallDownloader.Models
{
    public class ForgeDataModel
    {
        public Dictionary<string, List<string>> ImageSets { get; set; } = new();
        public List<ForgeEditionModel> Editions { get; set; }
        public List<MatchedSetModel> MatchedSets { get; set; }
        public List<Card> ImplementedCards { get; set; } = new();
        public List<Card> MissingCards { get; set; } = new();
    }
}
