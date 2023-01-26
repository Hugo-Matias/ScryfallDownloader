using Microsoft.EntityFrameworkCore;
using ScryfallDownloader.Data;
using ScryfallDownloader.Models;

namespace ScryfallDownloader.Services
{
    public class DataService
    {
        private readonly IDbContextFactory<DatabaseContext> _contextFactory;
        private readonly List<string> _mainSetTypes = new() { "core", "expansion" };
        private readonly List<string> _promoSetTypes = new() { "box", "promo", "token", "memorabilia" };

        public DataService(IDbContextFactory<DatabaseContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        //Sets

        public async Task<Set> Create(Set set)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            context.SetTypes.Attach(set.SetType);
            set.Cards = new List<Card>();
            var setEntity = await context.Sets.AddAsync(set);
            await context.SaveChangesAsync();
            return setEntity.Entity;
        }

        public async Task<Set?> GetSet(int id, bool includeCards = false)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            if (includeCards)
                return await context.Sets.Include(s => s.SetType)
                                   .Include(s => s.Cards).ThenInclude(c => c.Artist)
                                   .Include(s => s.Cards).ThenInclude(c => c.Rarity)
                                   .FirstOrDefaultAsync(s => s.SetId == id);
            else
                return await context.Sets.Include(s => s.SetType).FirstOrDefaultAsync(s => s.SetId == id);
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
                return await context.Sets.Include(s => s.SetType).FirstOrDefaultAsync(s => s.Code == code);
        }

        //Decks

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

        public async Task CreateDeckEntities(List<BaseDeckModel> decks, string sourceName, string sourceLink)
        {
            var deckIndex = 0;
            foreach (var deck in decks)
            {
                deckIndex++;
                if (await CheckDeckExists(deck.Name, deck.Player, sourceName, deck.Date)) continue;

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"{deckIndex}/{decks.Count}||{deck.Name} - {deck.Player} - {deck.Link.PathAndQuery}");
                Console.ResetColor();

                var deckEntity = new Deck();
                deckEntity.Name = deck.Name;
                deckEntity.Description = deck.Description;
                deckEntity.CreateDate = deck.Date;
                deckEntity.UpdateDate = DateTime.Now;
                deckEntity.Author = new Author() { Name = deck.Player };
                deckEntity.Format = new Format() { Name = deck.Format };
                deckEntity.Source = new Source() { Name = sourceName, Url = new Uri(sourceLink) };

                var createdDeckEntity = await Create(deckEntity);

                var cardIndex = 0;
                foreach (var card in deck.Cards)
                {
                    cardIndex++;
                    if (card.IsSideboard)
                        Console.ForegroundColor = ConsoleColor.DarkCyan;
                    Console.WriteLine($"{deckIndex}||{cardIndex}/{deck.Cards.Count}: {card.Name}");
                    Console.ResetColor();

                    var cardEntity = await GetLatestCard(card.Name, DateOnly.FromDateTime(deck.Date));

                    if (cardEntity != null) await AddCardToDeck(createdDeckEntity.DeckId, cardEntity.CardId, card.Quantity, card.IsSideboard);
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"----CARD NOT FOUND: {card.Name}");
                        Console.ResetColor();
                        if (deckEntity.MissingCards == null) deckEntity.MissingCards = new List<string>() { card.Name };
                        else deckEntity.MissingCards.Add(card.Name);
                    }
                }
                if (deckEntity.MissingCards != null) { var updatedDeck = await UpdateDeckMissingCards(deckEntity.DeckId, deckEntity.MissingCards); }
            }
        }

        public async Task<Deck?> GetDeck(string name)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.Decks.Include(d => d.Cards)
                                      .ThenInclude(c => c.Card)
                                      .ThenInclude(c => c.Set)
                                      .ThenInclude(s => s.SetType)
                                      .Include(d => d.Cards)
                                      .ThenInclude(c => c.Card)
                                      .ThenInclude(c => c.Layout)
                                      .Include(d => d.Author)
                                      .Include(d => d.Source)
                                      .Include(d => d.Format)
                                      .Include(d => d.Tags)
                                      .Include(d => d.Commander)
                                      .ThenInclude(c => c.Artist)
                                      .Include(d => d.Commander)
                                      .ThenInclude(c => c.Rarity)
                                      .AsSplitQuery()
                                      .FirstOrDefaultAsync(d => d.Name == name);
        }

        public async Task<Deck?> GetDeck(int deckId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.Decks.Include(d => d.Cards).ThenInclude(c => c.Card).ThenInclude(c => c.Set).ThenInclude(s => s.SetType)
                                      .Include(d => d.Cards).ThenInclude(c => c.Card).ThenInclude(c => c.Layout)
                                      .Include(d => d.Author)
                                      .Include(d => d.Source)
                                      .Include(d => d.Format)
                                      .Include(d => d.Tags)
                                      .Include(d => d.Commander).ThenInclude(c => c.Artist)
                                      .Include(d => d.Commander).ThenInclude(c => c.Rarity)
                                      .AsSplitQuery()
                                      .FirstOrDefaultAsync(d => d.DeckId == deckId);
        }

        public async Task<List<int>> GetDeckIdsBySource(int sourceId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.Decks.Where(d => d.Source.SourceId == sourceId).Select(d => d.DeckId).ToListAsync();
        }

        public async Task<Deck> UpdateDeckMissingCards(int deckId, ICollection<string> missingCards)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var deck = await context.Decks.FirstOrDefaultAsync(d => d.DeckId == deckId);
            deck.MissingCards = missingCards;
            await context.SaveChangesAsync();
            return deck;
        }

        public async Task<bool> CheckDeckExists(string name, DateTime createDate)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.Decks.AnyAsync(d => d.Name == name && d.CreateDate == createDate);
        }

        public async Task<bool> CheckDeckExists(string name, string? authorName, string source, DateTime createDate)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.Decks.AnyAsync(d => d.Name == name && d.CreateDate == createDate && d.Author.Name == authorName && d.Source.Name == source);
        }

        // Cards

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

        public async Task<Card?> GetCard(string name)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var card = await context.Cards.Include(c => c.Rarity)
                                .Include(c => c.Artist)
                                .Include(c => c.Set)
                                .Include(c => c.Layout)
                                .OrderByDescending(c => c.Set.ReleaseDate)
                                .FirstOrDefaultAsync(c => c.Name.ToLower().Contains(name.ToLower()));
            if (card == null)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Missing Card: {name}");
                Console.ResetColor();
            }
            return card;
        }

        public async Task<Card?> GetCard(string name, string setCode)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var card = await context.Cards.Include(c => c.Rarity)
                                .Include(c => c.Artist)
                                .Include(c => c.Set)
                                .Include(c => c.Layout)
                                .FirstOrDefaultAsync(c => c.Name.ToLower().Contains(name.ToLower()) && c.Set.Code.ToLower() == setCode.ToLower());
            if (card == null)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Missing Card: {name} | {setCode}");
                Console.ResetColor();
            }
            return card;
        }

        public async Task<Card?> GetLatestCard(string name, DateOnly date, bool isMainOnly = true)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            Card card = null;

            // Check for cards only in Core and Expansion sets
            if (isMainOnly)
            {
                card = await context.Cards.Include(c => c.Rarity).Include(c => c.Artist).Include(c => c.Layout).Include(c => c.Set).ThenInclude(s => s.SetType).OrderByDescending(c => c.Set.ReleaseDate).FirstOrDefaultAsync(c => c.Name.ToLower().Contains(name.ToLower()) && c.Set.ReleaseDate <= date && _mainSetTypes.Contains(c.Set.SetType.Name) && c.Set.ForgeCode != null);
                if (card != null) return card;
                else Console.WriteLine($"Card not found on MAIN sets: {name} | {date}");
            }

            // Check for cards in reprint and special sets but not in promos or gift set types
            card = await context.Cards.Include(c => c.Rarity).Include(c => c.Artist).Include(c => c.Layout).Include(c => c.Set).ThenInclude(s => s.SetType).OrderByDescending(c => c.Set.ReleaseDate).FirstOrDefaultAsync(c => c.Name.ToLower().Contains(name.ToLower()) && c.Set.ReleaseDate <= date && !_promoSetTypes.Contains(c.Set.SetType.Name) && c.Set.ForgeCode != null);
            if (card != null) return card;
            else Console.WriteLine($"Card not found on REPRINT sets: {name} | {date}");

            // Check latest card version before date without considering set type
            card = await context.Cards.Include(c => c.Rarity).Include(c => c.Artist).Include(c => c.Layout).Include(c => c.Set).ThenInclude(s => s.SetType).OrderByDescending(c => c.Set.ReleaseDate).FirstOrDefaultAsync(c => c.Name.ToLower().Contains(name.ToLower()) && c.Set.ReleaseDate <= date && c.Set.ForgeCode != null);
            if (card != null) return card;
            else Console.WriteLine($"Card not found before date: {name} | {date}");

            // If all else fails, get the latest version available of all time
            return await GetCard(name);
        }

        public async Task<List<Card>?> GetCards(string name)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.Cards.Include(c => c.Artist)
                                .Include(c => c.Rarity)
                                .Include(c => c.Layout)
                                .Include(c => c.Set)
                                .Where(c => c.Name.ToLower().Contains(name.ToLower()))
                                .ToListAsync();
        }

        public async Task<List<Card>?> GetUnimplementedCards()
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.Cards.Where(c => c.IsImplemented == false).ToListAsync();
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

        public async Task AddCardToSet(int setId, Card card)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var cardEntity = await context.Cards.Include(c => c.Artist).Include(c => c.Rarity).Include(c => c.Set).ThenInclude(s => s.SetType).FirstOrDefaultAsync(c => c.Name == card.Name && c.Set.SetId == setId && c.CollectorsNumber == card.CollectorsNumber);

            cardEntity ??= new();

            cardEntity.Name = card.Name;
            cardEntity.CollectorsNumber = card.CollectorsNumber;
            cardEntity.SetId = setId;
            cardEntity.IsImplemented = card.IsImplemented;
            cardEntity.ConvertedManaCost = card.ConvertedManaCost;
            cardEntity.IsHighres = card.IsHighres;
            cardEntity.ImageUrl = card.ImageUrl;
            cardEntity.Type = card.Type;
            cardEntity.ManaCost = card.ManaCost;
            cardEntity.Power = card.Power;
            cardEntity.Toughness = card.Toughness;
            cardEntity.Loyalty = card.Loyalty;
            cardEntity.LifeModifier = card.LifeModifier;
            cardEntity.HandModifier = card.HandModifier;

            var artistEntity = await context.Artists.FirstOrDefaultAsync(a => a.Name == card.Artist.Name);
            cardEntity.Artist = artistEntity != null ? artistEntity : card.Artist;

            var rarityEntity = await context.Rarities.FirstOrDefaultAsync(r => r.Name == card.Rarity.Name);
            cardEntity.Rarity = rarityEntity != null ? rarityEntity : card.Rarity;

            var layoutEntity = await context.Layouts.FirstOrDefaultAsync(l => l.Name == card.Layout.Name);
            cardEntity.Layout = layoutEntity != null ? layoutEntity : card.Layout;

            if (card.Keywords != null && card.Keywords.Count > 0)
            {
                var keywords = card.Keywords.ToList();
                foreach (var keyword in card.Keywords)
                {
                    if (await context.CardKeywords.AnyAsync(k => k.Keyword.KeywordId.Equals(keyword.KeywordId) && k.Card.CardId.Equals(cardEntity.CardId)))
                        keywords.Remove(keyword);
                    else context.Keywords.Attach(await context.Keywords.FirstOrDefaultAsync(k => k.KeywordId.Equals(keyword.KeywordId)));
                }
                cardEntity.Keywords = keywords;
            }

            if (card.Colors != null && card.Colors.Count > 0)
            {
                var colors = card.Colors.ToList();
                foreach (var color in card.Colors)
                {
                    if (await context.CardColors.AnyAsync(c => c.Color.ColorId.Equals(color.ColorId) && c.Card.CardId.Equals(cardEntity.CardId)))
                        colors.Remove(color);
                    else if (!context.Colors.Local.Any(c => c.ColorId.Equals(color.ColorId)))
                        context.Colors.Attach(await context.Colors.FirstOrDefaultAsync(c => c.ColorId.Equals(color.ColorId)));
                }
                cardEntity.Colors = colors;
            }

            if (card.ProducedColors != null && card.ProducedColors.Count > 0)
            {
                var colors = card.ProducedColors.ToList();
                foreach (var color in card.ProducedColors)
                {
                    if (await context.CardGenerateColors.AnyAsync(c => c.Color.ColorId.Equals(color.ColorId) && c.Card.CardId.Equals(cardEntity.CardId)))
                        colors.Remove(color);
                    else if (!context.Colors.Local.Any(c => c.ColorId.Equals(color.ColorId)))
                        context.Colors.Attach(await context.Colors.FirstOrDefaultAsync(c => c.ColorId.Equals(color.ColorId)));
                    //else context.Colors.Attach(color.Color);
                }
                cardEntity.ProducedColors = colors;
            }
            //var values = context.Entry(card).CurrentValues.Clone();
            //values[nameof(Card.CardId)] = cardEntity.CardId;
            //context.Entry(cardEntity).CurrentValues.SetValues(values);
            if (cardEntity.CardId <= 0) context.Cards.Add(cardEntity);
            await context.SaveChangesAsync();
        }

        public async Task AddCardsToSet(int setId, List<Card> cards)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var set = await context.Sets.Include(s => s.Cards).FirstOrDefaultAsync(s => s.SetId.Equals(setId));
            set.Cards = cards;
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

        public async Task AddCardsToDeck(Deck deck, List<DeckCard> cards)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            context.Decks.Attach(deck);
            deck.Cards = cards;
            await context.SaveChangesAsync();
        }

        public async Task AddCardsToDeck(Deck deck, List<(int, bool)> cards)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            context.Decks.Attach(deck);

            List<DeckCard> deckCards = new();
            foreach (var card in cards)
            {
                var exitingCard = deckCards.FirstOrDefault(c => c.CardId == card.Item1 && c.IsSideboard == card.Item2);
                if (exitingCard != null) { exitingCard.Quantity++; }
                else
                {
                    deckCards.Add(new DeckCard()
                    {
                        DeckId = deck.DeckId,
                        CardId = card.Item1,
                        Quantity = 1,
                        IsSideboard = card.Item2
                    });
                }
            }
            deck.Cards = deckCards;

            await context.SaveChangesAsync();
        }

        public async Task AddCommandersToDeck(int deckId, int commanderId, int commander2Id = -1)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var deck = await context.Decks.FirstOrDefaultAsync(d => d.DeckId == deckId);
            deck.Commander = await context.Cards.FirstOrDefaultAsync(c => c.CardId == commanderId);
            if (commander2Id > -1) { deck.Commander2 = await context.Cards.FirstOrDefaultAsync(c => c.CardId == commander2Id); }
            await context.SaveChangesAsync();
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

        // Card Keywords

        public async Task<Keyword> Create(Keyword keyword)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var entity = await context.Keywords.FirstOrDefaultAsync(k => k.Name == keyword.Name);
            if (entity == null)
            {
                context.Keywords.Add(keyword);
                await context.SaveChangesAsync();
                return await context.Keywords.FirstOrDefaultAsync(k => k.Name == keyword.Name);
            }
            else return entity;
        }

        public async Task<Keyword?> GetKeyword(string name)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.Keywords.FirstOrDefaultAsync(k => k.Name == name);
        }

        // Card Colors

        public async Task<Color> Create(Color color)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var entity = await context.Colors.FirstOrDefaultAsync(c => c.Symbol == color.Symbol);
            if (entity == null)
            {
                context.Colors.Add(color);
                await context.SaveChangesAsync();
                return await context.Colors.FirstOrDefaultAsync(c => c.Symbol == color.Symbol);
            }
            else return entity;
        }

        public async Task<Color?> GetColor(string symbol)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.Colors.FirstOrDefaultAsync(c => c.Symbol == symbol);
        }

        // Edhrec Commanders

        public async Task<EdhrecCommander> Create(EdhrecCommander commander)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var dbCommander = await context.EdhrecCommanders.FirstOrDefaultAsync(c => c.Name == commander.Name);
            if (dbCommander == null)
            {
                var commanderEntity = await context.EdhrecCommanders.AddAsync(commander);
                await context.SaveChangesAsync();
                return commanderEntity.Entity;
            }
            else return dbCommander;
        }

        public async Task<List<EdhrecCommander>> GetEdhrecCommanders()
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.EdhrecCommanders.ToListAsync();
        }

        public async Task<bool> CheckEdhrecCommandersPopulated()
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.EdhrecCommanders.AnyAsync();
        }

        // Set Types

        public async Task<SetType?> GetSetType(string name)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.SetTypes.Where(s => s.Name == name).FirstOrDefaultAsync();
        }

        // Database

        public async Task RecreateDatabase()
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            await context.Database.EnsureDeletedAsync();
            await context.Database.MigrateAsync();
        }
        public async Task<Setting> LoadSettings()
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            if (!await context.Settings.AnyAsync())
            {
                var setting = await context.Settings.AddAsync(new Setting());
                await context.SaveChangesAsync();
                return setting.Entity;
            }
            else return await context.Settings.AsNoTracking().OrderBy(s => s.SettingId).FirstAsync();
        }

        public async Task SaveSettings(Setting setting)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var settings = await context.Settings.OrderBy(s => s.SettingId).FirstAsync();
            var values = context.Entry(setting).CurrentValues.Clone();
            values[nameof(Setting.SettingId)] = settings.SettingId;
            context.Entry(settings).CurrentValues.SetValues(values);
            context.Entry(settings).State = EntityState.Modified;
            await context.SaveChangesAsync();
        }
    }
}
