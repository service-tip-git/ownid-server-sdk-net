using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using OwnID.Extensibility.Cache;
using OwnID.Extensibility.Configuration;
using OwnID.Extensibility.Flow.Contracts;
using OwnID.Extensions;

namespace OwnID.Services
{
    public class CookieService : ICookieService
    {
        private readonly IOwnIdCoreConfiguration _configuration;
        private readonly string _domain;

        public CookieService(IOwnIdCoreConfiguration configuration)
        {
            _configuration = configuration;
            EncryptionCookieName = string.Format(CookieNameTemplates.Encryption, _configuration.CookieReference);
            RecoveryCookieName = string.Format(CookieNameTemplates.Recovery, _configuration.CookieReference);
            CredIdCookieName = string.Format(CookieNameTemplates.CredId, _configuration.CookieReference);
            _domain = _configuration.OwnIdApplicationUrl.GetWebAppBaseDomain();
        }

        public string RecoveryCookieName { get; }

        public string EncryptionCookieName { get; }

        public string CredIdCookieName { get; }

        public CookieInfo CreateCookie(string name, string value)
        {
            return new()
            {
                Name = name,
                Value = value,
                Options = GetOptions(DateTimeOffset.Now.AddDays(_configuration.CookieExpiration))
            };
        }

        public CookieInfo DeleteCookie(string name)
        {
            return new()
            {
                Name = name,
                Remove = true,
                Options = GetOptions()
            };
        }

        public List<CookieInfo> CreateAuthCookies(CacheItem cacheItem)
        {
            var result = new List<CookieInfo>();

            if (!string.IsNullOrEmpty(cacheItem.EncToken))
                result.Add(CreateCookie(EncryptionCookieName,
                    $"{cacheItem.EncToken}{cacheItem.EncTokenEnding ?? string.Empty}"));

            if (!string.IsNullOrEmpty(cacheItem.RecoveryToken))
                result.Add(CreateCookie(RecoveryCookieName, cacheItem.RecoveryToken));

            if (!string.IsNullOrEmpty(cacheItem.Fido2CredentialId))
                result.Add(CreateCookie(CredIdCookieName, cacheItem.Fido2CredentialId));

            return result;
        }

        private CookieOptions GetOptions(DateTimeOffset? expire = null)
        {
            return new()
            {
                HttpOnly = true,
                Domain = _domain,
                Secure = !_configuration.IsDevEnvironment,
                SameSite = SameSiteMode.Lax,
                Expires = expire
            };
        }
    }
}