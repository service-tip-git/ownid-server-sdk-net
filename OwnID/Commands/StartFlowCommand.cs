using System.Threading.Tasks;
using OwnID.Cryptography;
using OwnID.Extensibility.Cache;
using OwnID.Extensibility.Flow.Contracts;
using OwnID.Extensions;
using OwnID.Services;

namespace OwnID.Commands
{
    // SetSecurityTokensCommand
    public class StartFlowCommand
    {
        private readonly ICacheItemRepository _cacheItemRepository;
        private readonly IJwtService _jwtService;

        public StartFlowCommand(ICacheItemRepository cacheItemRepository, IJwtService jwtService)
        {
            _cacheItemRepository = cacheItemRepository;
            _jwtService = jwtService;
        }

        public async Task<string> ExecuteAsync(Input input)
        {
            var respToken = _jwtService.GetJwtHash(input.ResponseJwt).EncodeBase64String();

            await _cacheItemRepository.UpdateAsync(input.Context, cacheItem =>
            {
                cacheItem.RequestToken = input.RequestToken;
                cacheItem.ResponseToken = respToken;
                cacheItem.RecoveryToken = input.RecoveryToken;

                if (!string.IsNullOrEmpty(input.EncryptionToken))
                {
                    var encToken = input.EncryptionToken;

                    if (encToken.EndsWith(CookieValuesConstants.PasscodeEnding))
                    {
                        encToken = encToken.Substring(0, encToken.Length - CookieValuesConstants.PasscodeEnding.Length);
                        cacheItem.EncTokenEnding = CookieValuesConstants.PasscodeEnding;
                    }

                    cacheItem.EncToken = encToken;
                }

                cacheItem.Fido2CredentialId = input.CredId;
            });

            return respToken;
        }

        public record Input
        {
            public string ResponseJwt { get; set; }

            public string Context { get; set; }

            public string CredId { get; set; }
            
            public string EncryptionToken { get; set; }

            public string RecoveryToken { get; set; }
            
            public string RequestToken { get; set; }
            
            public string ResponseToken { get; set; }
        }
    }
}