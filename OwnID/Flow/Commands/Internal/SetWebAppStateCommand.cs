using System;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using OwnID.Extensibility.Configuration;
using OwnID.Extensibility.Flow.Contracts;
using OwnID.Extensibility.Flow.Contracts.Internal;
using OwnID.Extensions;
using OwnID.Services;

namespace OwnID.Flow.Commands.Internal
{
    public class SetWebAppStateCommand
    {
        private readonly ICacheItemService _cacheItemService;
        private readonly IOwnIdCoreConfiguration _configuration;
        private readonly string _domain;

        public SetWebAppStateCommand(ICacheItemService cacheItemService, IOwnIdCoreConfiguration configuration)
        {
            _cacheItemService = cacheItemService;
            _configuration = configuration;
            EncryptionCookieName = string.Format(CookieNameTemplates.WebAppEncryption, _configuration.CookieReference);
            RecoveryCookieName = string.Format(CookieNameTemplates.WebAppRecovery, _configuration.CookieReference);
            _domain = _configuration.OwnIdApplicationUrl.GetWebAppBaseDomain();
        }

        public string RecoveryCookieName { get; }

        public string EncryptionCookieName { get; }

        public async Task<StateResult> ExecuteAsync(string context, StateRequest request)
        {
            var result = new StateResult();
            var encryptionToken = request.EncryptionToken;
            var recoveryToken = request.RecoveryToken;

            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Domain = _domain,
                Secure = !_configuration.IsDevEnvironment,
                SameSite = SameSiteMode.Lax,
                Expires = DateTimeOffset.Now.AddDays(_configuration.CookieExpiration)
            };

            if (string.IsNullOrWhiteSpace(request.EncryptionToken))
            {
                encryptionToken = Guid.NewGuid().ToString("N");
                using var rng = new RNGCryptoServiceProvider();
                var vector = new byte[16];
                rng.GetBytes(vector);
                var vectorValue = Convert.ToBase64String(vector);
                encryptionToken = $"{encryptionToken}{CookieValuesConstants.SubValueSeparator}{vectorValue}";

                if (_configuration.TFAEnabled && _configuration.Fido2FallbackBehavior == Fido2FallbackBehavior.Passcode)
                    encryptionToken = $"{encryptionToken}{CookieValuesConstants.PasscodeEnding}";

                result.Cookies.Add(new CookieInfo
                {
                    Name = EncryptionCookieName,
                    Value = encryptionToken,
                    Options = cookieOptions
                });
            }
            else if (encryptionToken.EndsWith(CookieValuesConstants.PasscodeEnding))
            {
                encryptionToken = encryptionToken.Substring(0,
                    encryptionToken.Length - CookieValuesConstants.PasscodeEnding.Length);
            }

            if (request.RequiresRecovery)
            {
                recoveryToken = string.IsNullOrWhiteSpace(request.RecoveryToken)
                    ? Guid.NewGuid().ToString("n")
                    : request.RecoveryToken;

                if (request.RecoveryToken != recoveryToken)
                    result.Cookies.Add(new CookieInfo
                    {
                        Name = RecoveryCookieName,
                        Value = recoveryToken,
                        Options = cookieOptions
                    });
            }

            await _cacheItemService.SetWebAppStateAsync(context, encryptionToken, recoveryToken);

            return result;
        }
    }
}