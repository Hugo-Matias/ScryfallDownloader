using System.Globalization;
using System.Text;

namespace ScryfallDownloader.Extensions
{
    public static class ParsingHelper
    {
        public static string ParseCardname(string str, bool removeIllegalChars = false)
        {
            //var newString = str.Replace('ü', 'u')
            //    .Replace('û', 'u')
            //    .Replace('ú', 'u')
            //    .Replace('â', 'a')
            //    .Replace('á', 'a')
            //    .Replace(@"  //  ", "");
            var newString = RemoveDiacritics(str.Replace(@" // ", ""));

            return removeIllegalChars ? CleanIllegalCharacters(newString) : newString;
        }

        private static string RemoveDiacritics(string text)
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

        private static string CleanIllegalCharacters(string filename) => Path.GetInvalidFileNameChars().Aggregate(filename, (current, c) => current.Replace(c.ToString(), string.Empty));

        /// <summary>
        /// Try parsing a string into an integer by removing non-digit characters.
        /// </summary>
        /// <param name="str"></param>
        /// <returns>An integer if successfully parsed, int.MinValue if not.</returns>
        public static int ParseToInt(string str)
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

    }
}
