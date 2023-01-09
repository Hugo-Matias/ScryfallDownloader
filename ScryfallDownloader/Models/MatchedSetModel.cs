namespace ScryfallDownloader.Models
{
    public class MatchedSetModel
    {
        public string ForgeCode { get; set; }
        public string ScryfallCode { get; set; }
        public int ForgeCount { get; set; }
        public int ScryfallCount { get; set; }
        public MatchedSetState State { get; set; } = MatchedSetState.Unknown;
    }

    public enum MatchedSetState
    {
        Unknown, Equal, Missing, Extra
    }
}
