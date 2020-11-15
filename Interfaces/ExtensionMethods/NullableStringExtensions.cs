using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;
using static System.String;

namespace Interfaces.ExtensionMethods
{
    public static class NullableStringExtensions
    {
        public static bool IsEmpty(this string? input)
        {
            return IsNullOrWhiteSpace(input);
        }

        public static bool IsNotEmpty(this string? input)
        {
            return !IsNullOrWhiteSpace(input);
        }

        public static bool IsLessThanMinLength(this string? input, int minLength)
        {
            return IsNullOrWhiteSpace(input) || input.Length < minLength;
        }

        private static bool IsEmailFormat(this string? input)
        {
            var regex = @"\A(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?)\Z";
            bool isValid = Regex.IsMatch(input, regex, RegexOptions.IgnoreCase);
            return isValid;
        }

        public static bool IsNotEmailFormat(this string? input)
        {
            return !IsEmailFormat(input);
        }

        public static bool IsValidJson(this string? strInput)
        {
            if (strInput == null)
            {
                return false;
            }
            strInput = strInput.Trim();
            if ((strInput.StartsWith("{") && strInput.EndsWith("}")) || //For object
                (strInput.StartsWith("[") && strInput.EndsWith("]"))) //For array
            {
                try
                {
                    // ReSharper disable once UnusedVariable
                    var obj = JToken.Parse(strInput);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        public static bool IsNotValidJson(this string? input)
        {
            return !IsValidJson(input);
        }
    }
}