using System.Text.Json.Serialization;

namespace OwnIdSdk.NetCore3.Contracts.Jwt
{
    /// <summary>
    ///     Wrapper for JWT base64 encoded string for transferring to OwnId application
    /// </summary>
    public class JwtContainer
    {
        public JwtContainer()
        {
        }
        
        public JwtContainer(string jwt)
        {
            Jwt = jwt;
        }
        
        [JsonPropertyName("jwt")] 
        public string Jwt { get; set; }
    }
}