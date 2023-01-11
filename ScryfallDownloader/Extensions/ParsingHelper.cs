namespace ScryfallDownloader.Extensions
{
    public static class ParsingHelper
    {
        public static string ParseCardname(string str, bool removeIllegalChars = false)
        {
            var newString = str.Replace("ü", "u")
                .Replace(@"  //  ", "");

            if (removeIllegalChars) newString = CleanIllegalCharacters(newString);

            return newString;
        }

        private static string CleanIllegalCharacters(string filename) => Path.GetInvalidFileNameChars().Aggregate(filename, (current, c) => current.Replace(c.ToString(), string.Empty));
    }
}
