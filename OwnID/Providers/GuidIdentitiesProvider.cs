using System;
using OwnID.Extensibility.Providers;
using OwnID.Extensions;

namespace OwnID.Providers
{
    /// <summary>
    ///     Provides Unique Identities based on GUID
    /// </summary>
    /// <inheritdoc cref="IIdentitiesProvider" />
    public class GuidIdentitiesProvider : IIdentitiesProvider
    {
        public string GenerateContext()
        {
            return Guid.NewGuid().ToShortString();
        }

        public string GenerateNonce()
        {
            return Guid.NewGuid().ToString();
        }

        public string GenerateUserId()
        {
            return Guid.NewGuid().ToString();
        }

        public string GenerateMagicLinkToken()
        {
            return Guid.NewGuid().ToShortString();
        }
    }
}