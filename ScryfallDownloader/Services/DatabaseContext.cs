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
        public DbSet<Keyword> Keywords { get; set; }
        public DbSet<CardKeyword> CardKeywords { get; set; }
        public DbSet<Color> Colors { get; set; }
        public DbSet<CardColor> CardColors { get; set; }
        public DbSet<CardGenerateColor> CardGenerateColors { get; set; }

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

            modelBuilder.Entity<DeckCard>().HasKey(e => new { e.DeckId, e.CardId, e.IsSideboard });
            modelBuilder.Entity<CardColor>().HasKey(e => new { e.CardId, e.ColorId });
            modelBuilder.Entity<CardGenerateColor>().HasKey(e => new { e.CardId, e.ColorId });
            modelBuilder.Entity<CardKeyword>().HasKey(e => new { e.CardId, e.KeywordId });

            modelBuilder.Entity<Card>().HasIndex(nameof(Card.Name), nameof(Card.Set.SetId), nameof(Card.CollectorsNumber)).IsUnique();
            modelBuilder.Entity<Rarity>().HasIndex(e => e.Name).IsUnique();
            modelBuilder.Entity<Artist>().HasIndex(e => e.Name).IsUnique();
            modelBuilder.Entity<Tag>().HasIndex(e => e.Name).IsUnique();
            modelBuilder.Entity<Author>().HasIndex(e => e.Name).IsUnique();
            modelBuilder.Entity<Source>().HasIndex(e => e.Name).IsUnique();
            modelBuilder.Entity<Format>().HasIndex(e => e.Name).IsUnique();
            modelBuilder.Entity<EdhrecCommander>().HasIndex(e => e.Name).IsUnique();
            modelBuilder.Entity<Keyword>().HasIndex(e => e.Name).IsUnique();
            modelBuilder.Entity<Color>().HasIndex(e => e.Name).IsUnique();
            modelBuilder.Entity<Set>().HasIndex(e => e.Code).IsUnique();
        }
    }
}
