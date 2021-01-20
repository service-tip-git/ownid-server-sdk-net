using System;
using System.Security.Cryptography;
using System.Threading.Tasks;
using OwnID.Extensibility.Cache;
using OwnID.Extensibility.Configuration;
using OwnID.Extensibility.Flow.Contracts.Cookies;
using OwnID.Services;

namespace OwnID.Commands
{
    public class SetNewEncryptionTokenCommand
    {
        private readonly ICacheItemRepository _cacheItemRepository;
        private readonly IOwnIdCoreConfiguration _coreConfiguration;

        public SetNewEncryptionTokenCommand(IOwnIdCoreConfiguration coreConfiguration,
            ICacheItemRepository cacheItemRepository)
        {
            _coreConfiguration = coreConfiguration;
            _cacheItemRepository = cacheItemRepository;
        }

        public async Task<CacheItem> ExecuteAsync(string context)
        {
            var encryptionToken = Guid.NewGuid().ToString("N");
            using var rng = new RNGCryptoServiceProvider();
            var vector = new byte[16];
            rng.GetBytes(vector);
            var vectorValue = Convert.ToBase64String(vector);

            return await _cacheItemRepository.UpdateAsync(context, item =>
            {
                item.EncKey = encryptionToken;
                item.EncVector = vectorValue;

                if (_coreConfiguration.TFAEnabled
                    && _coreConfiguration.Fido2FallbackBehavior == Fido2FallbackBehavior.Passcode)
                    item.AuthCookieType = CookieType.Passcode;
                else
                    item.AuthCookieType = CookieType.Basic;
            });
        }
    }
}