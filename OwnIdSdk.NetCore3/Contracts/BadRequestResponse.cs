using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace OwnIdSdk.NetCore3.Contracts
{
    public class BadRequestResponse
    {
        [JsonPropertyName("generalErrors")] 
        public IEnumerable<string> GeneralErrors { get; set; }

        [JsonPropertyName("fieldErrors")] 
        public IDictionary<string, IEnumerable<string>> FieldErrors { get; set; }
    }
}