using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using ScryfallDownloader.Data;
using System.Text.Json;

namespace ScryfallDownloader.Services
{
    public class DatabaseContext : DbContext
    {
        public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options) { }

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
        public DbSet<EdhrecCommander> EdhrecCommanders { get; set; }
        public DbSet<Setting> Settings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Uses Json serialization to store List<string>, the converter and comparer keep the domain class unclutered.
            // Doc: https://stackoverflow.com/a/52499249/12173765
            //      https://learn.microsoft.com/en-us/ef/core/modeling/value-comparers?tabs=ef5
            var splitStringConverter = new ValueConverter<ICollection<string>, string>(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                v => JsonSerializer.Deserialize<ICollection<string>>(v, (JsonSerializerOptions)null));

            var splitStringComparer = new ValueComparer<ICollection<string>>(
                (c1, c2) => new HashSet<string>(c1!).SetEquals(new HashSet<string>(c2!)),
                c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                c => c.ToList()
                );

            modelBuilder.Entity<Deck>().Property(nameof(Deck.MissingCards)).HasConversion(splitStringConverter, splitStringComparer);

            modelBuilder.Entity<DeckCard>().HasKey(dc => new { dc.DeckId, dc.CardId, dc.IsSideboard });

            modelBuilder.Entity<Rarity>().HasIndex(r => r.Name).IsUnique();
            modelBuilder.Entity<Artist>().HasIndex(a => a.Name).IsUnique();
            modelBuilder.Entity<Tag>().HasIndex(t => t.Name).IsUnique();
            modelBuilder.Entity<Author>().HasIndex(a => a.Name).IsUnique();
            modelBuilder.Entity<Source>().HasIndex(s => s.Name).IsUnique();
            modelBuilder.Entity<Format>().HasIndex(f => f.Name).IsUnique();
            modelBuilder.Entity<EdhrecCommander>().HasIndex(c => c.Name).IsUnique();
        }
    }
}
