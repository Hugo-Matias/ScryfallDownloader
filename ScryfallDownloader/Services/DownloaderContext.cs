using Microsoft.EntityFrameworkCore;
using ScryfallDownloader.Data;

namespace ScryfallDownloader.Services
{
    public class DownloaderContext : DbContext
    {
        public DownloaderContext(DbContextOptions<DownloaderContext> options) : base(options) { }

        public DbSet<Set> Sets { get; set; }
        public DbSet<Deck> Decks { get; set; }
        public DbSet<Card> Cards { get; set; }
        public DbSet<DeckCard> DeckCards { get; set; }
        public DbSet<SetType> SetTypes { get; set; }
        public DbSet<Rarity> Rarities { get; set; }
        public DbSet<Artist> Artists { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<Author> Authors { get; set; }
        public DbSet<Source> Sources { get; set; }
        public DbSet<Format> Formats { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //modelBuilder.Entity<DeckCard>().HasOne(i => i.Deck).WithMany(d => d.Cards);
            //modelBuilder.Entity<Set>().HasMany(s => s.Cards).WithOne();
            //modelBuilder.Entity<Card>().HasMany(c => c.Sets).WithOne();
            //modelBuilder.Entity<Card>().HasOne(c => c.Rarity).WithMany();

            modelBuilder.Entity<DeckCard>().HasKey(dc => new { dc.DeckId, dc.CardId });

            modelBuilder.Entity<Rarity>().HasIndex(r => r.Name).IsUnique();
            modelBuilder.Entity<Artist>().HasIndex(a => a.Name).IsUnique();
            modelBuilder.Entity<Tag>().HasIndex(t => t.Name).IsUnique();
            modelBuilder.Entity<Author>().HasIndex(a => a.Name).IsUnique();
            modelBuilder.Entity<Source>().HasIndex(s => s.Name).IsUnique();
            modelBuilder.Entity<Format>().HasIndex(f => f.Name).IsUnique();
        }
    }
}
