using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace OwnIdSdk.NetCore3.Extensibility.Json
{
    public static class OwnIdSerializer
    {
        private static readonly JsonSerializerOptions DefaultOptions = GetDefaultProperties();

        public static string Serialize(object data, JsonSerializerOptions options = null)
        {
            return JsonSerializer.Serialize(data, options ?? DefaultOptions);
        }

        public static T Deserialize<T>(string json, JsonSerializerOptions options = null)
        {
            return JsonSerializer.Deserialize<T>(json, options ?? DefaultOptions);
        }

        public static async Task<T> DeserializeAsync<T>(Stream jsonStream, JsonSerializerOptions options = null)
        {
            return await JsonSerializer.DeserializeAsync<T>(jsonStream, options ?? DefaultOptions);
        }

        public static JsonSerializerOptions GetDefaultProperties()
        {
            return new JsonSerializerOptions
            {
                Converters =
                    {new JsonStringEnumConverter(JsonNamingPolicy.CamelCase), new AutoPrimitiveToStringConverter()},
                IgnoreNullValues = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            ;
        }
    }
}