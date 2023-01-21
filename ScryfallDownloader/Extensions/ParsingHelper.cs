using System.Globalization;
using System.Text;

namespace ScryfallDownloader.Extensions
{
    public static class ParsingHelper
    {
        public static string ParseCardname(this string str, bool removeIllegalChars = false)
        {
            var newString = RemoveDiacritics(str.Replace(@" // ", ""));

            return removeIllegalChars ? CleanIllegalCharacters(newString) : newString;
        }

        public static string ParseSplitCardname(this string str, bool removeIllegalChars = false)
        {
            var newString = RemoveDiacritics(str.Split(@" // ")[0]);

            return removeIllegalChars ? CleanIllegalCharacters(newString) : newString;
        }

        private static string RemoveDiacritics(this string text)
        {
            var normalizedString = text.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();

            foreach (var c in normalizedString.EnumerateRunes())
            {
                var unicodeCategory = Rune.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
        }

        private static string CleanIllegalCharacters(this string filename) => Path.GetInvalidFileNameChars().Aggregate(filename, (current, c) => current.Replace(c.ToString(), string.Empty));

        /// <summary>
        /// Try parsing a string into an integer by removing non-digit characters.
        /// </summary>
        /// <param name="str"></param>
        /// <returns>An integer if successfully parsed, int.MinValue if not.</returns>
        public static int ParseToInt(this string str)
        {
            try
            {
                return int.Parse(new String(str.Where(Char.IsDigit).ToArray()));
            }
            catch (FormatException)
            {
                return int.MinValue;
            }
        }

        public static IEnumerable<string> SplitToLines(this string str)
        {
            if (string.IsNullOrWhiteSpace(str)) yield break;

            using (var reader = new StringReader(str))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    yield return line;
                }
            }
        }

        public static string ParseDeckFormat(this string format)
        {
            format = format.Trim().ToLower();

            switch (format)
            {
                case "cedh":
                    format = "commander";
                    break;

                case "canadian highlander":
                    format = "highlanderCanadian";
                    break;

                case "duel commander":
                    format = "duelCommander";
                    break;

                case "mtgo commander":
                    format = "mtgoCommander";
                    break;

                default:
                    break;
            }

            return format;
        }
    }
}
