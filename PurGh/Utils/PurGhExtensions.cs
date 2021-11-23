namespace PurGh
{
    using System;
    using System.IO;

    public static class PurGhExtensions
    {
        public static bool EqualsIgnoreCase(this string input, string compare)
        {
            return input?.Equals(compare, StringComparison.OrdinalIgnoreCase) == true;
        }

        public static bool StartsWithIgnoreCase(this string input, string subString)
        {
            return input?.StartsWith(subString, StringComparison.OrdinalIgnoreCase) == true;
        }

        public static bool EndsWithIgnoreCase(this string input, string subString)
        {
            return input?.EndsWith(subString, StringComparison.OrdinalIgnoreCase) == true;
        }

        public static bool ContainsIgnoreCase(this string input, string subString)
        {
            return input?.Contains(subString, StringComparison.OrdinalIgnoreCase) == true;
        }

        public static string GetFullPath(this string fileName)
        {
            return fileName == null
                ? null
                : Path.IsPathRooted(fileName) ? fileName : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), fileName);
        }
    }
}
