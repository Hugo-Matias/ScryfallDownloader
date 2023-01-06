using ScryfallApi.Client.Models;
using ScryfallDownloader.Models;
using System.Text.RegularExpressions;

namespace ScryfallDownloader.Services
{
    public class ForgeService
    {
        private readonly IOService _ioService;

        private string _imagesPath;
        private string _editionsPath;

        // Regex is much slower than simple string.Split() and should be avoided if possible.
        private Regex _patternMetadata = new Regex(@"(.+?)=(.+)", RegexOptions.Compiled);
        private Regex _patternCards = new Regex(@"([^ ]+) ([^ ]+) ([^@]+) @([^\n]+)", RegexOptions.Compiled);
        //private Regex _patternCards = new Regex(@"(.+?) (.+?) (.+) @(.+$)", RegexOptions.Compiled);

        public ForgeService(IOService ioService)
        {
            _ioService = ioService;
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
                    try
                    {
                        //var matches = _patternMetadata.Matches(line)[0].Groups;
                        //var field = matches[1].Value;
                        //var value = matches[2].Value;

                        var groups = line.Split('=');

                        // Some booster related information is included on it's own line, this check will ignore it and continue.
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
                    catch (ArgumentOutOfRangeException) { continue; }
                }

                else if (currentSection == "[cards]")
                {
                    if (model.Cards == null) model.Cards = new();

                    try
                    {
                        //var matches = _patternCards.Matches(line)[0].Groups;
                        //var number = matches[1].Value;
                        //var rarity = matches[2].Value;
                        //var name = matches[3].Value;
                        //var artist = matches[4].Value;

                        var groups = line.Split(" @");
                        var artist = groups.Length > 1 ? groups[1] : null;
                        groups = groups[0].Split(' ');
                        var number = groups[0];
                        var rarity = groups[1];
                        var name = string.Join(' ', groups.ToList().GetRange(2, groups.Length - 2));

                        ForgeCardModel card = new ForgeCardModel() { Number = number, Rarity = rarity, Name = name, Artist = artist };

                        model.Cards.Add(card);
                    }
                    catch (ArgumentOutOfRangeException) { continue; }
                }

                else if (currentSection == "[tokens]")
                {
                    if (model.Tokens == null) model.Tokens = new();
                    model.Tokens.Add(new ForgeCardModel() { Name = line });
                }

                else
                {
                    if (model.Other == null) model.Other = new();
                    if (!model.Other.ContainsKey(currentSection)) model.Other.Add(currentSection, new List<string>() { line });
                    else model.Other[currentSection].Add(line);
                }
            }

            return model;
        }

        public async Task AuditCards(ForgeDataModel data, List<Set> sets)
        {
            Dictionary<string, int> incompleteSets = new();

            data.ImageSets = GetLocalCardSets();
            data.Editions = await GetEditions();

            foreach (var edition in data.Editions)
            {
                List<string> editionCodes = new();

                editionCodes.Add(edition.Code.ToLower());
                if (edition.Code2 != null && !editionCodes.Contains(edition.Code2.ToLower())) editionCodes.Add(edition.Code2.ToLower());
                if (edition.Alias != null && !editionCodes.Contains(edition.Alias.ToLower())) editionCodes.Add(edition.Alias.ToLower());
                if (edition.ScryfallCode != null && !editionCodes.Contains(edition.ScryfallCode.ToLower())) editionCodes.Add(edition.ScryfallCode.ToLower());

                Console.WriteLine(string.Join(" ", editionCodes));
                if (!data.ImageSets.Any(s => editionCodes.Contains(s.Key))) { incompleteSets.Add(edition.Code, edition.Cards.Count); continue; }

                // If the local edition is not found on Scryfall. This typically means it's a custom one.
                if (sets.Count(s => s.Code.ToLower() == (edition.ScryfallCode != null ? edition.ScryfallCode.ToLower() : edition.Code.ToLower())) == 0) continue;

                var set = sets.Single(s => s.Code.ToLower() == (edition.ScryfallCode != null ? edition.ScryfallCode.ToLower() : edition.Code.ToLower()));
                var localSet = data.ImageSets.First(s => editionCodes.Contains(s.Key));
                //var localSet = data.ImageSets.Single(s => s.Key.ToLower() == edition.Code.ToLower());

                if (edition.Cards == null) { Console.Write(edition.Code); continue; }
                if (edition.Cards.Count > localSet.Value.Count) incompleteSets.Add(edition.Code, edition.Cards.Count - localSet.Value.Count);
            }

            var orderedSets = incompleteSets.OrderByDescending(s => s.Value);
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
    }
}
