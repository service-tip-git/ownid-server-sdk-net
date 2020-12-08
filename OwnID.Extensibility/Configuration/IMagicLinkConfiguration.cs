using System;

namespace OwnID.Extensibility.Configuration
{
    public interface IMagicLinkConfiguration
    {
        /// <summary>
        ///     Uri that will be used to redirect user with magic link
        /// </summary>
        public Uri RedirectUrl { get; set; }

        /// <summary>
        ///     Magic link token time-to-live
        /// </summary>
        /// <remarks>
        ///     In milliseconds. Default is 10 minutes
        /// </remarks>
        public uint TokenLifetime { get; set; }

        /// <summary>
        ///     Enables restriction that magic link should be used in the same browser it was requested
        /// </summary>
        /// <remarks>
        ///     Default is true
        /// </remarks>
        public bool SameBrowserUsageOnly { get; set; }
    }
}