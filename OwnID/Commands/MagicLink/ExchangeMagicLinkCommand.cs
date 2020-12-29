using System.Threading.Tasks;
using OwnID.Extensibility.Configuration;
using OwnID.Extensibility.Exceptions;
using OwnID.Extensibility.Flow.Contracts.MagicLink;
using OwnID.Flow.Adapters;
using OwnID.Services;

namespace OwnID.Commands.MagicLink
{
    public class ExchangeMagicLinkCommand
    {
        private readonly ICacheItemRepository _cacheItemRepository;
        private readonly IMagicLinkConfiguration _magicLinkConfiguration;
        private readonly IUserHandlerAdapter _userHandlerAdapter;

        public ExchangeMagicLinkCommand(ICacheItemRepository cacheItemRepository,
            IUserHandlerAdapter userHandlerAdapter, IMagicLinkConfiguration magicLinkConfiguration)
        {
            _cacheItemRepository = cacheItemRepository;
            _cacheItemRepository = cacheItemRepository;
            _userHandlerAdapter = userHandlerAdapter;
            _magicLinkConfiguration = magicLinkConfiguration;
        }

        public async Task<object> ExecuteAsync(ExchangeMagicLinkRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Context) || string.IsNullOrWhiteSpace(request.MagicToken)
                                                           || _magicLinkConfiguration.SameBrowserUsageOnly
                                                           && string.IsNullOrWhiteSpace(request.CheckToken))
                throw new CommandValidationException("Magic link required params were not provided");

            var cacheItem = await _cacheItemRepository.GetAsync(request.Context);

            if (_magicLinkConfiguration.SameBrowserUsageOnly && request.CheckToken
                != SendMagicLinkCommand.GetCheckToken(request.Context, request.MagicToken, cacheItem.DID))
                throw new CommandValidationException("Magic link wrong CheckToken");

            if (cacheItem.Payload != request.MagicToken)
                throw new CommandValidationException("Magic link wrong MagicToken");

            var result = await _userHandlerAdapter.OnSuccessLoginAsync(cacheItem.DID, null);
            await _cacheItemRepository.RemoveAsync(request.Context);
            return result;
        }
    }
}