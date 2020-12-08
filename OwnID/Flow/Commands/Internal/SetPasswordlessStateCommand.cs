using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using OwnID.Services;
using OwnID.Extensibility.Configuration;
using OwnID.Extensibility.Flow.Contracts;
using OwnID.Extensibility.Flow.Contracts.Internal;

namespace OwnID.Flow.Commands.Internal
{
    public class SetPasswordlessStateCommand
    {
        private readonly ICacheItemService _cacheItemService;
        private readonly IOwnIdCoreConfiguration _configuration;

        public SetPasswordlessStateCommand(ICacheItemService cacheItemService, IOwnIdCoreConfiguration configuration)
        {
            _cacheItemService = cacheItemService;
            _configuration = configuration;
            EncryptionCookieName = string.Format(CookieNameTemplates.PasswordlessEncryption, _configuration.CookieReference);
            RecoveryCookieName = string.Format(CookieNameTemplates.PasswordlessRecovery, _configuration.CookieReference);
            CredIdCookieName = string.Format(CookieNameTemplates.PasswordlessCredId, _configuration.CookieReference);
        }

        public string RecoveryCookieName { get; }

        public string EncryptionCookieName { get; }
        
        public string CredIdCookieName { get; }

        public async Task<StateResult> ExecuteAsync(string context, StateRequest request)
        {
            var result = new StateResult();

            string recoveryToken = null;

            var encryptionToken = string.IsNullOrWhiteSpace(request.EncryptionToken)
                ? Guid.NewGuid().ToString("N")
                : request.EncryptionToken;

            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Domain = _configuration.TopDomain,
                Secure = !_configuration.IsDevEnvironment,
                Expires = DateTimeOffset.Now.AddYears(_configuration.CookieExpiration),
                SameSite = SameSiteMode.Strict
            };

            result.Cookies.Add(new CookieInfo
            {
                Name = EncryptionCookieName,
                Value = encryptionToken,
                Options = cookieOptions
            });

            if (request.RequiresRecovery)
            {
                recoveryToken = string.IsNullOrWhiteSpace(request.RecoveryToken)
                    ? Guid.NewGuid().ToString("n")
                    : request.RecoveryToken;

                result.Cookies.Add(new CookieInfo
                {
                    Name = RecoveryCookieName,
                    Value = recoveryToken,
                    Options = cookieOptions
                });
            }

            if (!string.IsNullOrWhiteSpace(request.CredId))
            {
                result.Cookies.Add(new CookieInfo
                {
                    Name = CredIdCookieName,
                    Value = request.CredId,
                    Options = cookieOptions
                });
            }

            await _cacheItemService.SetPasswordlessStateAsync(context, encryptionToken, recoveryToken);

            return result;
        }
    }
}