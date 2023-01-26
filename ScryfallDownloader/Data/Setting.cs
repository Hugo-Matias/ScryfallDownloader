namespace ScryfallDownloader.Data
{
    public class Setting
    {
        public int SettingId { get; set; }
        public int MT8Page { get; set; } = 368;
        public string SCGDate { get; set; } = "20-01-2023";
        public int SCGDeck { get; set; } = 16300;
        public int SCGLimit { get; set; } = 20;
        public int SCGPage { get; set; } = 816;
        public string EDHCommander { get; set; } = "atraxa-praetors-voice";
        public int EDHDeck { get; set; } = 3592;
        public int ARCHPage { get; set; } = 1;
        public string WTFDeck { get; set; }
        public int ImportSet { get; set; } = 609;
    }
}
