using System;
using OwnID.Extensibility.Configuration;

namespace OwnID.Configuration
{
    public class MagicLinkConfiguration : IMagicLinkConfiguration
    {
        public Uri RedirectUrl { get; set; }

        public uint TokenLifetime { get; set; }

        public bool SameBrowserUsageOnly { get; set; } = true;
    }
}