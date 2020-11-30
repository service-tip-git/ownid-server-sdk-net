using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace OwnID.Extensions
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
        
        public static string GetSha256(this string str)
        {
            using var sha256 = new SHA256Managed();

            var b64 = Encoding.UTF8.GetBytes(str);
            var hash = sha256.ComputeHash(b64);

            return Convert.ToBase64String(hash);
        }
    }
}