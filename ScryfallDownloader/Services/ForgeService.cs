using ScryfallApi.Client;
using ScryfallApi.Client.Models;
using ScryfallDownloader.Extensions;
using ScryfallDownloader.Models;
using System.Text.RegularExpressions;

namespace ScryfallDownloader.Services
{
    public class ForgeService
    {
        private readonly IOService _ioService;
        private readonly ScryfallApiClient _api;
        private readonly DataDownloaderService _dataDownloader;
        private string _imagesPath;
        private string _editionsPath;
        private List<Card> _cards;

        public ForgeService(IOService ioService, ScryfallApiClient api, DataDownloaderService dataDownloader)
        {
            _ioService = ioService;
            _api = api;
            _dataDownloader = dataDownloader;
        }

        public void SetPaths(string imagesPath, string editionsPath)
        {
            _imagesPath = imagesPath;
            _editionsPath = editionsPath;
        }

        public Dictionary<string, List<string>> GetLocalCardSets()
        {
            Dictionary<string, List<string>> sets = new();

            foreach (var dir in _ioService.GetRelativeDirectories(_imagesPath))
            {
                var path = Path.Combine(_imagesPath, dir);
                var files = _ioService.GetFilesInDirectory(path).Select(s => Path.GetRelativePath(path, s)).ToList();
                sets.Add(dir, files);
            }

            return sets;
        }

        public async Task<List<ForgeEditionModel>> GetEditions()
        {
            List<ForgeEditionModel> editions = new();
            //List<string[]> editionsText = new();

            var editionFiles = _ioService.GetFilesInDirectory(_editionsPath);

            foreach (var edition in editionFiles)
            {
                var text = await _ioService.ReadTextFile(edition);
                editions.Add(ParseEdition(text));
                //editionsText.Add(text);
            }

            //ParseEditionInfoForDevelopment(editionsText);

            return editions;
        }

        private ForgeEditionModel ParseEdition(string[] edition)
        {
            string currentSection = string.Empty;
            ForgeEditionModel model = new();

            foreach (var line in edition)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                if (line.StartsWith("[")) { currentSection = line; continue; }

                if (currentSection == "[metadata]")
                {
                    var groups = line.Split('=');

                    // Some booster related information is included on it's own line that doesn't follow the fields pattern,
                    // This check will ignore it and continue, throwing exception otherwise.
                    if (groups.Length < 2) continue;

                    var field = groups[0];
                    var value = groups[1];

                    if (field.ToLower() == "code") model.Code = value;
                    if (field.ToLower() == "scryfallcode") model.ScryfallCode = value;
                    if (field.ToLower() == "name") model.Name = value;
                    if (field.ToLower() == "type") model.Type = value;
                    if (field.ToLower() == "date") model.Date = DateOnly.Parse(value);
                    if (field.ToLower() == "code2") model.Code2 = value;
                    if (field.ToLower() == "alias") model.Alias = value;
                }

                else if (currentSection == "[cards]")
                {
                    if (model.Cards == null) model.Cards = new();

                    var card = ParseCardLine(line, currentSection);

                    if (card != null) model.Cards.Add(card);
                }

                else if (currentSection == "[tokens]")
                {
                    if (model.Tokens == null) model.Tokens = new();
                    model.Tokens.Add(new ForgeCardModel() { Name = line });
                }

                else
                {
                    // Misc. sections are parsed here,
                    // If the current line fits the card pattern and produces a ForgeCard object it's added to the cards
                    // Otherwise add the line to a new section in the Others property dictionary
                    var card = ParseCardLine(line, currentSection);

                    if (card != null)
                    {
                        if (model.Cards == null) model.Cards = new();
                        model.Cards.Add(card);
                    }
                    else
                    {
                        if (model.Other == null) model.Other = new();
                        if (!model.Other.ContainsKey(currentSection)) model.Other.Add(currentSection, new List<string>() { line });
                        else model.Other[currentSection].Add(line);
                    }
                }
            }

            return model;
        }

        private List<char> _cardRarities = new List<char>() { 'L', 'C', 'U', 'R', 'M', 'S' };
        private StringSplitOptions _splitOptions = StringSplitOptions.RemoveEmptyEntries;

        private ForgeCardModel ParseCardLine(string line, string section)
        {
            if (section != "[cards]")
            {
                if (line.Contains('|')) return null;
            }

            string number, rarity, name, artist;
            number = rarity = name = artist = string.Empty;

            // Artist names are prefixed by " @" and all the card lines that include an artist follow the same default pattern:
            // CN R N @A
            // CN - card_number, R - rarity (single character), N - card name, A - artist(s)
            // This shouldn't throw any out of range exceptions.
            if (line.Contains(" @"))
            {
                var groups = line.Split(" @");
                artist = groups.Length > 1 ? groups[1] : null;
                groups = groups[0].Split(' ', 3, _splitOptions);
                number = groups[0];
                rarity = groups[1];
                name = groups[2];
            }
            else
            {
                string[] groups;

                if (_cardRarities.Any(r => line.StartsWith($"{r} ")))
                {
                    groups = line.Split(' ', 2, _splitOptions);
                    rarity = groups[0];
                    name = groups[1];
                }

                else
                {
                    if (section != "[cards]") return null;

                    groups = line.Split(' ', 3, _splitOptions);

                    if (groups.Length > 1)
                    {
                        number = groups[0];
                        rarity = groups[1];
                        name = groups[2];
                    }
                    // This excludes all lines with a single word
                    // None of them had relevant card data and all were on a non-[cards] section
                    else { return null; }
                }
            }

            ForgeCardModel card = new ForgeCardModel() { Number = number, Rarity = rarity, Name = name, Artist = artist };

            return card;
        }

        public async Task AuditSets(ForgeDataModel data, List<Set> sets)
        {
            data.ImageSets = GetLocalCardSets();
            data.Editions = await GetEditions();
            data.MatchedSets = MatchSetCodes(data, sets);

            //CheckUnimplementedForDevelopment(data);
        }

        private List<MatchedSetModel> MatchSetCodes(ForgeDataModel data, List<Set> sets)
        {
            List<MatchedSetModel> matchedSets = new();

            foreach (var edition in data.Editions)
            {
                MatchedSetModel match = new();

                if (!string.IsNullOrWhiteSpace(edition.ScryfallCode))
                {
                    // Check if the ScryfallCode set mentioned in the Edition file actually exists
                    if (!sets.Any(s => s.Code.ToLower() == edition.ScryfallCode.ToLower())) Console.WriteLine($"MISSING SET: {edition.ScryfallCode}");
                    else match.ScryfallCode = edition.ScryfallCode.ToLower();
                }
                else
                {
                    // If the Edition doesn't indicate any ScryfallCode, try Code instead
                    if (sets.Any(s => s.Code.ToLower() == edition.Code.ToLower())) match.ScryfallCode = edition.Code.ToLower();
                }

                // If an Edition file includes a Code2 field, that's what the game will use as pic folder path, it uses Code otherwise.
                if (!string.IsNullOrWhiteSpace(match.ScryfallCode))
                    match.ForgeCode = (edition.Code2 != null ? edition.Code2 : edition.Code).ToLower();

                // Get card count stats for both sets
                if (sets.Any(s => s.Code == match.ScryfallCode))
                    match.ScryfallCount = sets.First(s => s.Code == match.ScryfallCode).card_count;

                if (!string.IsNullOrWhiteSpace(match.ForgeCode) && data.ImageSets.ContainsKey(match.ForgeCode))
                    match.ForgeCount = data.ImageSets[match.ForgeCode].Count;

                if (match.ScryfallCount > 0)
                {
                    if (match.ScryfallCount == match.ForgeCount) match.State = MatchedSetState.Equal;
                    else if (match.ScryfallCount > match.ForgeCount) match.State = MatchedSetState.Missing;
                    else if (match.ScryfallCount < match.ForgeCount) match.State = MatchedSetState.Extra;
                }

                if (!string.IsNullOrWhiteSpace(match.ForgeCode) || !string.IsNullOrWhiteSpace(match.ScryfallCode))
                    matchedSets.Add(match);
            }

            return matchedSets;
        }

        public async Task AuditCards(ForgeDataModel data, string code, bool redownload, bool ignoreFoil, bool ignoreVariation, bool ignoreNonEnglish)
        {
            await GetCardsData(redownload);

            // Some Scryfall sets match to multiple Forge editions (eg. MED), to include every possible card we need to iterate over them.
            var matchedSets = data.MatchedSets.Where(s => s.ScryfallCode == code);

            // Prevents from downloading unimplemented sets. Temporary Solution.
            //if (matchedSets == null) return;

            var forgeEditions = data.Editions.Where(s => (s.Code2 != null && matchedSets.Count(m => m.ForgeCode == s.Code2.ToLower()) > 0) || matchedSets.Count(m => m.ForgeCode == s.Code.ToLower()) > 0);

            var matchedCards = _cards.Where(c => c.Set == code).ToList();
            var foils = new List<Card>();
            var specialCN = "dps★";

            // First OrderBy integer converted CN, this will enable natural sorting for all the cards, including the ones with special chars.
            // ThenBy, will make sure identical CN's with special characters will be ordered correctly (eg. 1, 1★).
            foreach (var card in matchedCards.OrderBy(c => ParsingHelper.ParseToInt(c.CollectorNumber)).ThenBy(c => c.CollectorNumber))
            {
                // TODO: Better logic to handle foils,
                // Current implementation works for some particular sets where Scryfall's data doesn't match with Forge editions (eg. 10E)
                //if (ignoreFoil && card.Finishes.Contains("foil")) { foils.Add(card); continue; }
                if (ignoreFoil && card.CollectorNumber.Any(specialCN.Contains)) { foils.Add(card); continue; }

                // Has with foils, in some cases, Scryfall's set will contain more cards than specified in the Forge edition,
                // This will lead to variation indexing (eg. Forest1, Forest2, etc.), the card image will appear missing ingame due to wrong filename
                // At this moment this issue is more relevant on older sets and should be considered on a per-set basis.
                if (ignoreNonEnglish && card.Language != "en") continue;
                if (ignoreVariation && card.Variation) continue;

                bool isImplemented = false;
                foreach (var set in forgeEditions)
                {
                    isImplemented = set.Cards.Any(c => ParsingHelper.ParseCardname(card.Name).Contains(ParsingHelper.ParseCardname(c.Name)));
                    if (isImplemented) break;
                }

                // Melded card tokens (eg. from EMN) aren't loaded from the edition as a card but are used in-game from the same path
                if (isImplemented || (!isImplemented && card.Layout == "meld")) data.ImplementedCards.Add(card);
                else data.MissingCards.Add(card);
            }
        }

        public async Task GetCardsData(bool redownload = false)
        {
            if (_cards != null && _cards.Count > 0 && !redownload) return;

            _cards = _ioService.GetCardsData();

            if (_cards == null || redownload)
            {
                var bulkData = await _api.BulkData.Get();
                var defaultCards = bulkData.Data.First(x => x.Type == "default_cards");
                await _dataDownloader.DownloadJsonData(defaultCards.DownloadUri.LocalPath);
                _cards = _ioService.GetCardsData();
            }
        }

        /// <summary>
        /// For development only!
        /// Writes and counts [section] information from the edition txt files. Useful for model definition.
        /// </summary>
        /// <param name="editions"></param>
        private void ParseEditionInfoForDevelopment(List<string[]> editions)
        {
            Dictionary<string, int> _sections = new();
            Dictionary<string, int> _metadataFields = new();
            string currentSection = string.Empty;

            foreach (var edition in editions)
            {
                foreach (var line in edition)
                {
                    if (line.StartsWith("["))
                    {
                        if (!_sections.ContainsKey(line)) _sections.Add(line, 1);
                        else _sections[line]++;
                    }

                    if (currentSection == "[metadata]")
                    {
                        Regex pattern = new Regex(@"(.+?)=(.+)");

                        try
                        {
                            var matches = pattern.Matches(line)[0].Groups;
                            var field = matches[1].Value;
                            var value = matches[2].Value;
                            if (!_metadataFields.ContainsKey(field)) _metadataFields.Add(field, 1);
                            else _metadataFields[field]++;
                        }
                        catch (ArgumentOutOfRangeException) { Console.WriteLine(line); }
                    }
                }
            }

            File.WriteAllLines("_sections.txt", _sections.OrderByDescending(x => x.Value).Select(x => $"{x.Key} - {x.Value}").ToArray());
            File.WriteAllLines("_metadataFields.txt", _metadataFields.OrderByDescending(x => x.Value).Select(x => $"{x.Key} - {x.Value}").ToArray());
        }
        /// <summary>
        /// For development only!
        /// Checks implemented card scripts and groups them by Set Type. Use ingame type counts in Deck Editor.
        /// </summary>
        private void CheckUnimplementedForDevelopment(ForgeDataModel data)
        {
            List<string> implemented = new();
            List<string> unimplemented = new();

            var cardsPath = @"E:\Jogos\Magic the Gathering\Forge\res\cardsfolder\cardsfolder";

            foreach (var dir in Directory.GetDirectories(cardsPath))
            {
                foreach (var file in Directory.GetFiles(dir, "*.txt"))
                {
                    var lines = File.ReadAllLines(file);

                    var name = lines.First(l => l.StartsWith("Name:")).Split(':', 2, _splitOptions)[1];
                    implemented.Add(name.ToLower());
                }
            }

            int cardCount = 0;
            var orderedSets = data.Editions.OrderBy(s => s.Type);

            Dictionary<string, int> types = new();

            foreach (var edition in orderedSets)
            {
                if (!types.ContainsKey(edition.Type))
                {
                    types.Add(edition.Type, 0);
                }
                foreach (var card in edition.Cards)
                {
                    var cardName = card.Name.Contains(" // ") ? card.Name.Split(" // ")[0] : card.Name;
                    if (implemented.Contains(cardName.ToLower()))
                        types[edition.Type]++;
                    else
                        unimplemented.Add($"{edition.Type} | {edition.Code} - {card.Name}");
                }

                if (edition.Type != "Other") cardCount += edition.Cards.Count;
                //Console.WriteLine($"{edition.Type} | {edition.Code}: {edition.Name} - {edition.Cards.Count}");
            }

            File.WriteAllLines("_unimplemented.txt", unimplemented);

            foreach (var k in types)
            {
                Console.WriteLine($"{k.Key} - {k.Value}");
            }
        }
    }
}
