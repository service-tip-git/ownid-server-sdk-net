using System.Text.Json;
using System.Text.RegularExpressions;

namespace OwnIdSdk.NetCore3.Extensions
{
    internal static class StringExtensions
    {
        public static string ToCamelCase(this string input)
        {
            return JsonNamingPolicy.CamelCase.ConvertName(input);
        }
        
        public static string SanitizeCookieName(this string cookieName)
        {
            if (string.IsNullOrWhiteSpace(cookieName))
                return cookieName;

            var pattern = new Regex("[()<>@,;:\\/\"[\\]?={}]+");
            return pattern.Replace(cookieName, "-");
        }
    }
}