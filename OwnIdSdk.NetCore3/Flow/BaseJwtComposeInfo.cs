using System;
using OwnIdSdk.NetCore3.Flow.Steps;

namespace OwnIdSdk.NetCore3.Flow
{
    public class BaseJwtComposeInfo
    {
        /// <summary>
        ///     WebApp encryption passphrase
        /// </summary>
        public string EncryptionPassphrase { get; set; }

        /// <summary>
        ///     Response locale
        /// </summary>
        public string Locale { get; set; }

        /// <summary>
        ///     Include requester data
        /// </summary>
        public bool IncludeRequester { get; set; }

        /// <summary>
        ///     User browser time
        /// </summary>
        public DateTime? ClientTime { get; set; }

        /// <summary>
        ///     Auth process unique identifier
        /// </summary>
        public string Context { get; set; }

        /// <summary>
        ///     Possible behavior for WebApp
        /// </summary>
        public FrontendBehavior Behavior { get; set; }
    }
}