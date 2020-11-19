using System;
using System.Text.RegularExpressions;
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

        public bool IsContextFormatValid(string context)
        {
            return Regex.IsMatch(context, "^([a-zA-Z0-9_-]{22})$");
        }

        public string GenerateUserId()
        {
            return Guid.NewGuid().ToString();
        }
    }
}