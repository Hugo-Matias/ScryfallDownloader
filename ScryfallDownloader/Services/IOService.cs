namespace ScryfallDownloader.Services
{
    public class IOService
    {
        public Task SaveFile(string filePath, byte[] data) => File.WriteAllBytesAsync(filePath, data);

        public DirectoryInfo CreateDirectory(string path) => Directory.CreateDirectory(path);
    }
}
