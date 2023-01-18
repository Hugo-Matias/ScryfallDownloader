using ScryfallApi.Client.Models;
using System.Text.Json;

namespace ScryfallDownloader.Services
{
    public class IOService
    {
        public Task<string[]> ReadTextFile(string filePath) => File.ReadAllLinesAsync(filePath);

        public Task SaveTextFile(string filePath, string data) => File.WriteAllTextAsync(filePath, data);

        public Task SaveFile(string filePath, byte[] data) => File.WriteAllBytesAsync(filePath, data);

        public DirectoryInfo CreateDirectory(string path) => Directory.CreateDirectory(path);

        public List<string> GetRelativeDirectories(string path) => Directory.GetDirectories(path).Select(d => Path.GetRelativePath(path, d)).ToList();

        public List<string> GetFilesInDirectory(string path) => Directory.GetFiles(path).ToList();

        public List<Card>? GetCardsData()
        {
            if (!File.Exists("cards.json")) return null;
            return JsonSerializer.Deserialize<List<Card>>(File.ReadAllText("cards.json"));
        }
    }
}
