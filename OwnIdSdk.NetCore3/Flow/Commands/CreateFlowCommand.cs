using System;
using System.Threading.Tasks;
using OwnIdSdk.NetCore3.Extensibility.Configuration;
using OwnIdSdk.NetCore3.Extensibility.Flow;
using OwnIdSdk.NetCore3.Extensibility.Flow.Contracts;
using OwnIdSdk.NetCore3.Extensibility.Providers;
using OwnIdSdk.NetCore3.Flow.Adapters;
using OwnIdSdk.NetCore3.Services;

namespace OwnIdSdk.NetCore3.Flow.Commands
{
    public class CreateFlowCommand
    {
        private readonly ICacheItemService _cacheItemService;
        private readonly uint _expiration;
        private readonly IIdentitiesProvider _identitiesProvider;
        private readonly IAccountLinkHandlerAdapter _linkHandlerAdapter;
        private readonly IUrlProvider _urlProvider;

        public CreateFlowCommand(ICacheItemService cacheItemService, IUrlProvider urlProvider,
            IIdentitiesProvider identitiesProvider, IOwnIdCoreConfiguration coreConfiguration,
            IAccountLinkHandlerAdapter linkHandlerAdapter = null)
        {
            _cacheItemService = cacheItemService;
            _urlProvider = urlProvider;
            _identitiesProvider = identitiesProvider;
            _linkHandlerAdapter = linkHandlerAdapter;
            _expiration = coreConfiguration.CacheExpirationTimeout;
        }
        
        public async Task<GetChallengeLinkResponse> ExecuteAsync(GenerateContextRequest request)
        {
            var challengeContext = _identitiesProvider.GenerateContext();
            var nonce = _identitiesProvider.GenerateNonce();

            string did = null;
            string payload = null;
            FlowType flowType;

            switch (request.Type)
            {
                case ChallengeType.Register:
                case ChallengeType.Login:
                    flowType = !request.IsPartial ? FlowType.Authorize : FlowType.PartialAuthorize;
                    break;
                case ChallengeType.Link:
                    if (_linkHandlerAdapter == null)
                        throw new NotSupportedException("Account Link feature was not enabled but was invoked");

                    did = await _linkHandlerAdapter.GetCurrentUserIdAsync(request.Payload.ToString());
                    flowType = request.IsQr ? FlowType.LinkWithPin : FlowType.Link;
                    break;
                case ChallengeType.Recover:
                    payload = request.Payload.ToString();
                    flowType = request.IsQr ? FlowType.RecoverWithPin : FlowType.Recover;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            await _cacheItemService.CreateAuthFlowSessionItemAsync(challengeContext, nonce, request.Type, flowType,
                did,
                payload);

            return new GetChallengeLinkResponse(challengeContext
                , _urlProvider.GetWebAppWithCallbackUrl(_urlProvider.GetStartFlowUrl(challengeContext)).ToString()
                , nonce
                , _expiration);
        }
    }
}