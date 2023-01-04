namespace ScryfallDownloader.States
{
    public class DownloadStates
    {
        private bool _isDownloading = false;
        private int _currentCard;

        public bool IsDownloading
        {
            get => _isDownloading;
            set
            {
                if (value != _isDownloading)
                {
                    _isDownloading = value;
                    OnDownloadChanged?.Invoke();
                }
            }
        }

        public int TotalCards { get; set; }

        public int CurrentCard
        {
            get => _currentCard;
            set
            {
                _currentCard = value;
                OnCardDownloaded?.Invoke();
            }
        }

        public string CurrentCardName { get; set; }

        public event Action OnDownloadChanged;
        public event Action OnCardDownloaded;
    }
}
