using System.Text.Json.Serialization;

namespace OwnIdSdk.Tools.Configurator
{
    public class Config
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }
        
        [JsonPropertyName("description")]
        public string Description { get; set; }
        
        [JsonPropertyName("icon")]
        public string Icon { get; set; }
        
        [JsonPropertyName("callback_url")]
        public string CallbackUrl { get; set; }
        
        [JsonPropertyName("overwrite_fields")]
        public bool OverwriteFields { get; set; }

        [JsonPropertyName("cache_type")]
        public string CacheType => "redis";
        
        [JsonPropertyName("cache_config")]
        public string RedisConnection { get; set; }
        
        [JsonPropertyName("pub_key")]
        public string PublicKey { get; set; }
        
        [JsonPropertyName("private_key")]
        public string PrivateKey { get; set; }
        
        [JsonPropertyName("did")]
        public string DID { get; set; }
    }

    public class GigyaConfig
    {
        [JsonPropertyName("data_center")]
        public string DataCenter { get; set; }
        
        [JsonPropertyName("api_key")]
        public string ApiKey { get; set; }
        
        [JsonPropertyName("user_key")]
        public string UserKey { get; set; }
        
        [JsonPropertyName("secret")]
        public string Secret { get; set; }
        
        [JsonPropertyName("login_type")]
        public string LoginType { get; set; }
    }
}