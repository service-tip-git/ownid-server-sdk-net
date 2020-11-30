using System;
using OwnID.Flow.Steps;

namespace OwnID.Flow
{
    public class BaseJwtComposeInfo
    {
        /// <summary>
        ///     User data encryption token
        /// </summary>
        public string EncToken { get; set; }

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

        /// <summary>
        ///     Identifies if connection can be recovered
        /// </summary>
        public bool CanBeRecovered { get; set; }
    }
}