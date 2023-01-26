using ScryfallDownloader.Services;

namespace ScryfallDownloader.Repositories
{
    public class DownloaderRepository
    {
        private readonly MoxfieldDownloaderService _moxfield;
        private readonly StarCityGamesDownloaderService _scg;
        private readonly MtgTop8DownloaderService _mtgtop8;
        private readonly EdhrecDownloaderService _edhrec;
        private readonly ArchidektDownloaderService _arch;
        private readonly MtgWtfDownloaderService _mtgwtf;

        public DownloaderRepository(MoxfieldDownloaderService moxfield, StarCityGamesDownloaderService scg, MtgTop8DownloaderService mtgtop8, EdhrecDownloaderService edhrec, ArchidektDownloaderService arch, MtgWtfDownloaderService mtgwtf)
        {
            _moxfield = moxfield;
            _scg = scg;
            _mtgtop8 = mtgtop8;
            _edhrec = edhrec;
            _arch = arch;
            _mtgwtf = mtgwtf;
        }

        public async Task MoxfieldDownload() => await _moxfield.Download();
        public async Task ScgDownload() => await _scg.Download();
        public async Task Mtgtop8Download() => await _mtgtop8.Download();
        public async Task EdhrecDownload() => await _edhrec.Download();
        public async Task ArchDownload() => await _arch.Download();
        public async Task MtgWtfDownload() => await _mtgwtf.Download();
    }
}
