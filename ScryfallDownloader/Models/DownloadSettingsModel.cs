using ScryfallApi.Client.Models;
using System.ComponentModel.DataAnnotations;

namespace ScryfallDownloader.Models
{
    public class DownloadSettingsModel
    {
        [Required]
        public string? ImagesPath { get; set; }
        [Required]
        public string? EditionsPath { get; set; }
        /// <summary>
        /// List of selected Sets for download
        /// </summary>
        public List<string> Sets { get; set; } = new();
        /// <summary>
        /// List of fetched cards from the selected sets
        /// </summary>
        public List<Card> Cards { get; set; } = new();
        public bool ConvertToJpg { get; set; } = true;
        public int OutputQuality { get; set; } = 75;
        public string Format { get; set; } = "png";
        public bool IgnoreExisting { get; set; } = false;
        public bool RemoveExisting { get; set; } = false;
        public bool RedownloadData { get; set; } = false;
    }
}
