using System.Text.Json;

namespace OwnIdSdk.NetCore3.Extensions
{
    internal static class StringExtensions
    {
        public static string ToCamelCase(this string input)
        {
            return JsonNamingPolicy.CamelCase.ConvertName(input);
        }
    }
}