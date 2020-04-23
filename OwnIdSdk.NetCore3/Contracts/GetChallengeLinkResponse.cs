using System.Text.Json.Serialization;

namespace OwnIdSdk.NetCore3.Contracts
{
    public class GetChallengeLinkResponse
    {
        public GetChallengeLinkResponse(string context, string url, string nonce)
        {
            Context = context;
            Url = url;
            Nonce = nonce;
        }

        [JsonPropertyName("url")]
        public string Url { get; }
        
        [JsonPropertyName("context")]
        public string Context { get; }
        
        [JsonPropertyName("nonce")]
        public string Nonce { get; }
    }
}