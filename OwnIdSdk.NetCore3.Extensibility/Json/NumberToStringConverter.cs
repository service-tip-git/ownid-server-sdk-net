using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OwnIdSdk.NetCore3.Extensibility.Json
{
    public class AutoPrimitiveToStringConverter : JsonConverter<string>
    {
        public override bool CanConvert(Type typeToConvert)
        {
            return typeof(string) == typeToConvert;
        }

        public override string Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            switch (reader.TokenType)
            {
                case JsonTokenType.Number:
                    return reader.TryGetInt64(out var l)
                        ? l.ToString()
                        : reader.GetDouble().ToString(CultureInfo.InvariantCulture);
                case JsonTokenType.String:
                    return reader.GetString();
                case JsonTokenType.True:
                    return reader.GetString();
                case JsonTokenType.False:
                    return reader.GetString();
                default:
                {
                    using var document = JsonDocument.ParseValue(ref reader);
                    return document.RootElement.Clone().ToString();
                }
            }
        }

        public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value);
        }
    }
}