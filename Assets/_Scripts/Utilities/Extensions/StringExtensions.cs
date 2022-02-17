namespace Utilities.Extensions
{
    public static class StringExtensions
    {
        public static string Capitalize(this string input) =>
            input switch
            {
                null => string.Empty,
                "" => string.Empty,
                _ => input[0].ToString().ToUpper() + input.Substring(1)
            };

        public static bool IsNullOrEmpty(this string input) => string.IsNullOrEmpty(input);
    }
}