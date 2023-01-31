using System.ComponentModel.DataAnnotations.Schema;

namespace ScryfallDownloader.Data
{
    public class Deck
    {
        public int DeckId { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public Author Author { get; set; }
        public Source Source { get; set; }
        public Format Format { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime? UpdateDate { get; set; }
        public ICollection<DeckTag>? Tags { get; set; }
        public int? CommanderCardId { get; set; }
        [ForeignKey(nameof(CommanderCardId))]
        public Card? Commander { get; set; }
        public int? ViewCount { get; set; }
        public int? LikeCount { get; set; }
        public int? CommentCount { get; set; }
        public ICollection<DeckCard>? Cards { get; set; }
        public ICollection<string>? MissingCards { get; set; }
        public int? Commander2CardId { get; set; }
        [ForeignKey(nameof(Commander2CardId))]
        public Card? Commander2 { get; set; }
    }
}
