using ScryfallApi.Client.Models;
using System.ComponentModel.DataAnnotations;

namespace ScryfallDownloader.Models
{
    public class DownloadSettingsModel
    {
        [Required]
        public string? ImagesPath { get; set; }
        public List<string> Sets { get; set; } = new();
        public List<Card> Cards { get; set; } = new();
        public bool ConvertToJpg { get; set; } = false;
        public int OutputQuality { get; set; } = 75;
        public string Format { get; set; } = "png";
    }
}
