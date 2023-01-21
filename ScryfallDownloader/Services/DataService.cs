using Microsoft.EntityFrameworkCore;
using ScryfallDownloader.Data;

namespace ScryfallDownloader.Services
{
    public class DataService
    {
        private readonly IDbContextFactory<DatabaseContext> _contextFactory;
        private readonly ForgeService _forge;

        public DataService(IDbContextFactory<DatabaseContext> contextFactory, ForgeService forge)
        {
            _contextFactory = contextFactory;
            _forge = forge;
        }

        public async Task<Set> Create(Set set)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            context.SetTypes.Attach(set.SetType);
            set.Cards = new List<Card>();
            var setEntity = await context.Sets.AddAsync(set);
            await context.SaveChangesAsync();
            return setEntity.Entity;
        }

        public async Task<Deck> Create(Deck deck)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            if (deck.Tags != null)
            {
                var tagsList = new List<Tag>();
                foreach (var tag in deck.Tags)
                {
                    var tagEntity = await context.Tags.FirstOrDefaultAsync(t => t.Name == tag.Name);
                    if (tagEntity != null)
                    {
                        context.Tags.Attach(tagEntity);
                        tagsList.Add(tagEntity);
                    }
                    else tagsList.Add(tag);
                }
                deck.Tags = tagsList;
            }

            var author = await context.Authors.FirstOrDefaultAsync(a => a.Name == deck.Author.Name);
            if (author != null)
            {
                context.Authors.Attach(author);
                deck.Author = author;
            }

            var format = await context.Formats.FirstOrDefaultAsync(f => f.Name == deck.Format.Name);
            if (format != null)
            {
                context.Formats.Attach(format);
                deck.Format = format;
            }

            var source = await context.Sources.FirstOrDefaultAsync(s => s.Name == deck.Source.Name);
            if (source != null)
            {
                context.Sources.Attach(source);
                deck.Source = source;
            }

            if (deck.Commander != null)
            {
                var commander = await context.Cards.FirstOrDefaultAsync(c => c.Name == deck.Commander.Name);
                if (commander != null)
                {
                    context.Cards.Attach(commander);
                    deck.Commander = commander;
                }
            }

            var deckEntity = await context.Decks.AddAsync(deck);
            await context.SaveChangesAsync();
            return deckEntity.Entity;
        }

        public async Task<Card> Create(Card card)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var rarity = await context.Rarities.FirstOrDefaultAsync(r => r.Name == card.Rarity.Name);
            if (rarity != null)
            {
                context.Rarities.Attach(rarity);
                card.Rarity = rarity;
            }

            var artist = await context.Artists.FirstOrDefaultAsync(a => a.Name == card.Artist.Name);
            if (artist != null)
            {
                context.Artists.Attach(artist);
                card.Artist = artist;
            }

            var set = await context.Sets.FirstOrDefaultAsync(s => s.Code == card.Set.Code);
            if (set != null)
            {
                context.Sets.Attach(set);
                card.Set = set;
            }

            var cardEntity = await context.Cards.AddAsync(card);
            await context.SaveChangesAsync();
            return cardEntity.Entity;
        }

        public async Task<Set?> GetSet(string code, bool includeCards = false)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            if (includeCards)
                return await context.Sets.Include(s => s.SetType)
                                   .Include(s => s.Cards).ThenInclude(c => c.Artist)
                                   .Include(s => s.Cards).ThenInclude(c => c.Rarity)
                                   .FirstOrDefaultAsync(s => s.Code == code);
            else
                return await context.Sets.FirstOrDefaultAsync(s => s.Code == code);
        }

        public async Task<SetType?> GetSetType(string name)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.SetTypes.Where(s => s.Name == name).FirstOrDefaultAsync();
        }

        public async Task<Card?> GetCard(string name)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.Cards.Include(c => c.Rarity)
                                .Include(c => c.Artist)
                                .Include(c => c.Set)
                                .OrderByDescending(c => c.Set.ReleaseDate)
                                .FirstOrDefaultAsync(c => c.Name.ToLower().Contains(name.ToLower()));
        }

        public async Task<Card?> GetCard(string name, string setCode)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.Cards.Include(c => c.Rarity)
                                .Include(c => c.Artist)
                                .Include(c => c.Set)
                                .FirstOrDefaultAsync(c => c.Name.ToLower().Contains(name.ToLower()) && c.Set.Code == setCode);
        }

        public async Task<Card?> GetCard(string name, DateOnly date)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.Cards.Include(c => c.Rarity)
                                .Include(c => c.Artist)
                                .Include(c => c.Set)
                                .OrderByDescending(c => c.Set.ReleaseDate)
                                .FirstOrDefaultAsync(c => c.Name.ToLower().Contains(name.ToLower()) && c.Set.ReleaseDate <= date);
        }

        public async Task<List<Card>?> GetCards(string name)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.Cards.Include(c => c.Artist)
                                .Include(c => c.Rarity)
                                .Include(c => c.Set)
                                .Where(c => c.Name.ToLower().Contains(name.ToLower()))
                                .ToListAsync();
        }

        public async Task<List<Card>?> GetUnimplementedCards()
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.Cards.Where(c => c.IsImplemented == false).ToListAsync();
        }

        public async Task<Card?> UpdateCardImplementation(int cardId, bool isImplemented)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var card = await context.Cards.FirstOrDefaultAsync(c => c.CardId == cardId);
            if (card != null)
            {
                card.IsImplemented = isImplemented;
                await context.SaveChangesAsync();
            }
            return card;
        }

        public async Task<Deck?> GetDeck(string name)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.Decks.Include(d => d.Cards).ThenInclude(c => c.Card).ThenInclude(c => c.Set)
                                .Include(d => d.Author)
                                .Include(d => d.Source)
                                .Include(d => d.Format)
                                .Include(d => d.Tags)
                                .Include(d => d.Commander).ThenInclude(c => c.Artist)
                                .Include(d => d.Commander).ThenInclude(c => c.Rarity)
                                .FirstOrDefaultAsync(d => d.Name == name);
        }

        public async Task<bool> CheckDeckExists(string name, DateTime createDate)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.Decks.AnyAsync(d => d.Name == name && d.CreateDate == createDate);
        }

        public async Task<bool> CheckDeckExists(string name, string authorName, DateTime createDate)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.Decks.AnyAsync(d => d.Name == name && d.CreateDate == createDate && d.Author.Name == authorName);
        }

        public async Task<Deck> UpdateDeckMissingCards(int deckId, ICollection<string> missingCards)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var deck = await context.Decks.FirstOrDefaultAsync(d => d.DeckId == deckId);
            deck.MissingCards = missingCards;
            await context.SaveChangesAsync();
            return deck;
        }

        public async Task AddCardToSet(int setId, int cardId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var set = await context.Sets.Include(s => s.Cards).FirstOrDefaultAsync(s => s.SetId.Equals(setId));
            var card = await context.Cards.FirstOrDefaultAsync(s => s.CardId.Equals(cardId));

            set.Cards.Add(card);
            card.Set = set;
            await context.SaveChangesAsync();
        }

        public async Task AddCardToDeck(int deckId, int cardId, int quantity, bool isSideboard)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var deckCardEntity = await context.DeckCards.FirstOrDefaultAsync(dc => dc.DeckId == deckId && dc.CardId == cardId && dc.IsSideboard == isSideboard);

            if (deckCardEntity != null)
            {
                deckCardEntity.Quantity += quantity;
                await context.SaveChangesAsync();
                return;
            }

            var deckCard = new DeckCard();
            var deck = await context.Decks.FirstOrDefaultAsync(c => c.DeckId == deckId);
            var card = await context.Cards.FirstOrDefaultAsync(c => c.CardId == cardId);

            deckCard.Deck = deck;
            deckCard.Card = card;
            deckCard.Quantity = quantity;
            deckCard.IsSideboard = isSideboard;

            context.DeckCards.Add(deckCard);

            await context.SaveChangesAsync();
        }

        public async Task RecreateDatabase()
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            await context.Database.EnsureDeletedAsync();
            await context.Database.MigrateAsync();
        }
    }
}
