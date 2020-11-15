namespace Utilities.Extensions
{
    public static class StringExtensions
    {
        public static string? SafeTrim(this string input)
        {
            return input?.Trim();
        }

        public static bool IsBlank(this string input)
        {
            return string.IsNullOrWhiteSpace(input);
        }

        public static bool IsNotBlank(this string input)
        {
            return !string.IsNullOrWhiteSpace(input);
        }
    }
}