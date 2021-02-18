using System.Text.RegularExpressions;

namespace WebScraper.Extensions
{
    public static class StringExtensions
    {
        public static string ParseDigitsFromText(this string text)
        {
            return Regex.Replace(text, @"[^\d]", string.Empty);
        }
    }
}