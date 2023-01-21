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

        public Set Create(Set set)
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                context.SetTypes.Attach(set.SetType);
                set.Cards = new List<Card>();
                var setEntity = context.Sets.Add(set);
                context.SaveChanges();
                return setEntity.Entity;
            }
        }
        public Deck Create(Deck deck)
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                if (deck.Tags != null)
                {
                    var tagsList = new List<Tag>();
                    foreach (var tag in deck.Tags)
                    {
                        var tagEntity = context.Tags.FirstOrDefault(t => t.Name == tag.Name);
                        if (tagEntity != null)
                        {
                            context.Tags.Attach(tagEntity);
                            tagsList.Add(tagEntity);
                        }
                        else tagsList.Add(tag);
                    }
                    deck.Tags = tagsList;
                }

                var author = context.Authors.FirstOrDefault(a => a.Name == deck.Author.Name);
                if (author != null)
                {
                    context.Authors.Attach(author);
                    deck.Author = author;
                }

                var format = context.Formats.FirstOrDefault(f => f.Name == deck.Format.Name);
                if (format != null)
                {
                    context.Formats.Attach(format);
                    deck.Format = format;
                }

                var source = context.Sources.FirstOrDefault(s => s.Name == deck.Source.Name);
                if (source != null)
                {
                    context.Sources.Attach(source);
                    deck.Source = source;
                }

                if (deck.Commander != null)
                {
                    var commander = context.Cards.FirstOrDefault(c => c.Name == deck.Commander.Name);
                    if (commander != null)
                    {
                        context.Cards.Attach(commander);
                        deck.Commander = commander;
                    }
                }

                var deckEntity = context.Decks.Add(deck).Entity;
                context.SaveChanges();
                return deckEntity;
            }
        }

        public Card Create(Card card)
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                var rarity = context.Rarities.FirstOrDefault(r => r.Name == card.Rarity.Name);
                if (rarity != null)
                {
                    context.Rarities.Attach(rarity);
                    card.Rarity = rarity;
                }

                var artist = context.Artists.FirstOrDefault(a => a.Name == card.Artist.Name);
                if (artist != null)
                {
                    context.Artists.Attach(artist);
                    card.Artist = artist;
                }

                var set = context.Sets.FirstOrDefault(s => s.Code == card.Set.Code);
                if (set != null)
                {
                    context.Sets.Attach(set);
                    card.Set = set;
                }

                var entity = context.Cards.Add(card).Entity;

                context.SaveChanges();

                return entity;
            }
        }

        public Set GetSet(string code, bool includeCards = false)
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                if (includeCards)
                    return context.Sets.Include(s => s.SetType)
                                       .Include(s => s.Cards).ThenInclude(c => c.Artist)
                                       .Include(s => s.Cards).ThenInclude(c => c.Rarity)
                                       .FirstOrDefault(s => s.Code == code);
                else
                    return context.Sets.FirstOrDefault(s => s.Code == code);
            }
        }

        public SetType GetSetType(string name)
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                return context.SetTypes.Where(s => s.Name == name).FirstOrDefault();
            }
        }

        public Card GetCard(string name)
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                return context.Cards.Include(c => c.Rarity)
                                    .Include(c => c.Artist)
                                    .Include(c => c.Set)
                                    .OrderByDescending(c => c.Set.ReleaseDate)
                                    .FirstOrDefault(c => c.Name.ToLower().Contains(name.ToLower()));
            }
        }

        public Card GetCard(string name, string setCode)
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                return context.Cards.Include(c => c.Rarity)
                                    .Include(c => c.Artist)
                                    .Include(c => c.Set)
                                    .FirstOrDefault(c => c.Name.ToLower().Contains(name.ToLower()) && c.Set.Code == setCode);
            }
        }

        public Card GetCard(string name, DateOnly date)
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                return context.Cards.Include(c => c.Rarity)
                                    .Include(c => c.Artist)
                                    .Include(c => c.Set)
                                    .OrderByDescending(c => c.Set.ReleaseDate)
                                    .FirstOrDefault(c => c.Name.ToLower().Contains(name.ToLower()) && c.Set.ReleaseDate <= date);
            }
        }

        public List<Card> GetCards(string name)
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                return context.Cards.Include(c => c.Artist)
                                    .Include(c => c.Rarity)
                                    .Include(c => c.Set)
                                    .Where(c => c.Name.ToLower().Contains(name.ToLower()))
                                    .ToList();
            }
        }

        public List<Card> GetUnimplementedCards()
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                return context.Cards.Where(c => c.IsImplemented == false).ToList();
            }
        }

        public Card UpdateCardImplementation(int cardId, bool isImplemented)
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                var card = context.Cards.FirstOrDefault(c => c.CardId == cardId);
                if (card != null)
                {
                    card.IsImplemented = isImplemented;
                    context.SaveChanges();
                }
                return card;
            }
        }

        public Deck GetDeck(string name)
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                return context.Decks.Include(d => d.Cards).ThenInclude(c => c.Card).ThenInclude(c => c.Set)
                                    .Include(d => d.Author)
                                    .Include(d => d.Source)
                                    .Include(d => d.Format)
                                    .Include(d => d.Tags)
                                    .Include(d => d.Commander).ThenInclude(c => c.Artist)
                                    .Include(d => d.Commander).ThenInclude(c => c.Rarity)
                                    .FirstOrDefault(d => d.Name == name);
            }
        }

        public bool CheckDeckExists(string name, DateTime createDate)
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                return context.Decks.Any(d => d.Name == name && d.CreateDate == createDate);
            }
        }

        public bool CheckDeckExists(string name, string authorName, DateTime createDate)
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                return context.Decks.Any(d => d.Name == name && d.CreateDate == createDate && d.Author.Name == authorName);
            }
        }

        public Deck UpdateDeckMissingCards(int deckId, ICollection<string> missingCards)
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                var deck = context.Decks.FirstOrDefault(d => d.DeckId == deckId);
                deck.MissingCards = missingCards;
                context.SaveChanges();
                return deck;
            }
        }

        public void AddCardToSet(int setId, int cardId)
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                var set = context.Sets.Include(s => s.Cards).FirstOrDefault(s => s.SetId.Equals(setId));
                var card = context.Cards.FirstOrDefault(s => s.CardId.Equals(cardId));

                set.Cards.Add(card);
                card.Set = set;
                context.SaveChanges();
            }
        }

        public void AddCardToDeck(int deckId, int cardId, int quantity, bool isSideboard)
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                var deckCardEntity = context.DeckCards.FirstOrDefault(dc => dc.DeckId == deckId && dc.CardId == cardId && dc.IsSideboard == isSideboard);

                if (deckCardEntity != null)
                {
                    deckCardEntity.Quantity += quantity;
                    context.SaveChanges();
                    return;
                }

                var deckCard = new DeckCard();
                var deck = context.Decks.FirstOrDefault(c => c.DeckId == deckId);
                var card = context.Cards.FirstOrDefault(c => c.CardId == cardId);

                deckCard.Deck = deck;
                deckCard.Card = card;
                deckCard.Quantity = quantity;
                deckCard.IsSideboard = isSideboard;

                context.DeckCards.Add(deckCard);

                context.SaveChanges();
            }
        }

        public void RecreateDatabase()
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                context.Database.EnsureDeleted();
                context.Database.Migrate();
            }
        }
    }
}
