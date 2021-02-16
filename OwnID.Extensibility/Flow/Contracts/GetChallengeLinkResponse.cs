using System.Text.Json.Serialization;

namespace OwnID.Extensibility.Flow.Contracts
{
    /// <summary>
    ///     POST /ownid/ response body structure
    /// </summary>
    public class GetChallengeLinkResponse
    {
        /// <summary>
        ///     Create instance of <see cref="GetChallengeLinkResponse" /> with required parameters
        /// </summary>
        /// <param name="context">Value for <see cref="Context" />. Context identifier</param>
        /// <param name="url">Value for <see cref="Url" />. Url for qr or link that leads to OwnId app</param>
        /// <param name="nonce">Value for <see cref="Nonce" /></param>
        /// <param name="expiration">Context expiration</param>
        /// <param name="magicLinkEnabled">Magic link feature flag</param>
        public GetChallengeLinkResponse(string context, string url, string nonce, uint expiration, bool magicLinkEnabled)
        {
            Context = context;
            Url = url;
            Nonce = nonce;
            Expiration = expiration;
            Config = new ChallengeConfig
            {
                MagicLinkEnabled = magicLinkEnabled
            };
        }

        /// <summary>
        ///     Url for qr or link that leads to OwnId app
        /// </summary>
        public string Url { get; }

        /// <summary>
        ///     Context identifier
        /// </summary>
        public string Context { get; }

        /// <summary>
        ///     Generated nonce
        /// </summary>
        public string Nonce { get; }

        /// <summary>
        ///     Expiration
        /// </summary>
        public uint Expiration { get; set; }

        /// <summary>
        ///     Configuration
        /// </summary>
        public ChallengeConfig Config { get; set; }

        public class ChallengeConfig
        {
            [JsonPropertyName("magicLink")]
            public bool MagicLinkEnabled { get; set; }
            
            public string LogLevel { get; set; }
        }
    }
}