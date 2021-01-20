using System.Linq;
using System.Threading.Tasks;
using OwnID.Cryptography;
using OwnID.Extensibility.Flow;
using OwnID.Extensibility.Flow.Contracts.Cookies;
using OwnID.Extensions;
using OwnID.Services;

namespace OwnID.Commands
{
    // SetSecurityTokensCommand
    public class StartFlowCommand
    {
        private readonly ICacheItemRepository _cacheItemRepository;
        private readonly ICookieService _cookieService;
        private readonly IJwtService _jwtService;

        public StartFlowCommand(ICacheItemRepository cacheItemRepository, IJwtService jwtService,
            ICookieService cookieService)
        {
            _cacheItemRepository = cacheItemRepository;
            _jwtService = jwtService;
            _cookieService = cookieService;
        }

        public async Task<string> ExecuteAsync(Input input)
        {
            var respToken = _jwtService.GetJwtHash(input.ResponseJwt).EncodeBase64String();

            await _cacheItemRepository.UpdateAsync(input.Context, cacheItem =>
            {
                cacheItem.RequestToken = input.RequestToken;
                cacheItem.ResponseToken = respToken;
                cacheItem.RecoveryToken = _cookieService.GetVersionedCookieValues(input.RecoveryTokenCookieValue).Values
                    ?.First();

                // ignore fido2 credential for recover because it will be recreated
                var authCookie = _cookieService.GetVersionedCookieValues(!string.IsNullOrEmpty(input.CredIdCookieValue)
                    ? input.CredIdCookieValue
                    : input.EncryptionTokenCookieValue);

                if (authCookie.Type.HasValue)
                    cacheItem.AuthCookieType = authCookie.Type.Value;

                if (authCookie.Type == CookieType.Fido2 && cacheItem.ChallengeType != ChallengeType.Recover)
                    cacheItem.Fido2CredentialId = authCookie.Values?.First();
                else if ((authCookie.Type == CookieType.Basic || authCookie.Type == CookieType.Passcode)
                         && authCookie.Values?.Length == 2)
                {
                    cacheItem.EncKey = authCookie.Values[0];
                    cacheItem.EncVector = authCookie.Values[1];
                }
            });

            return respToken;
        }

        public record Input
        {
            public string ResponseJwt { get; set; }

            public string Context { get; set; }

            public string CredIdCookieValue { get; set; }

            public string EncryptionTokenCookieValue { get; set; }

            public string RecoveryTokenCookieValue { get; set; }

            public string RequestToken { get; set; }

            public string ResponseToken { get; set; }
        }
    }
}