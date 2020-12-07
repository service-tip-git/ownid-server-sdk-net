using System;
using System.Threading.Tasks;
using OwnID.Services;
using OwnID.Extensibility.Configuration;
using OwnID.Extensibility.Flow;
using OwnID.Extensibility.Flow.Abstractions;
using OwnID.Extensibility.Flow.Contracts;
using OwnID.Extensibility.Providers;

namespace OwnID.Flow.Commands
{
    public class CreateFlowCommand
    {
        private readonly ICacheItemService _cacheItemService;
        private readonly IOwnIdCoreConfiguration _configuration;
        private readonly IIdentitiesProvider _identitiesProvider;
        private readonly IAccountLinkHandler _linkHandler;
        private readonly IUrlProvider _urlProvider;

        public CreateFlowCommand(ICacheItemService cacheItemService, IUrlProvider urlProvider,
            IIdentitiesProvider identitiesProvider, IOwnIdCoreConfiguration configuration,
            IAccountLinkHandler linkHandler = null)
        {
            _cacheItemService = cacheItemService;
            _urlProvider = urlProvider;
            _identitiesProvider = identitiesProvider;
            _configuration = configuration;
            _linkHandler = linkHandler;
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
                    if (_linkHandler == null)
                        throw new NotSupportedException("Account Link feature was not enabled but was invoked");

                    var state = await _linkHandler.GetCurrentUserLinkStateAsync(request.Payload.ToString());

                    if (state.ConnectedDevicesCount >= _configuration.MaximumNumberOfConnectedDevices)
                        return new GetChallengeLinkResponse(default, _urlProvider.GetWebAppConnectionsUrl().ToString(),
                            default, default);

                    did = state.DID;
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
                did, payload);

            var startFlowUrl = _urlProvider.GetStartFlowUrl(challengeContext);
            var destinationUrl = _urlProvider.GetWebAppSignWithCallbackUrl(startFlowUrl, request.Language);

            if (_configuration.TFAEnabled
                && (flowType == FlowType.PartialAuthorize
                    || request.Type == ChallengeType.Link
                    || request.Type == ChallengeType.Recover))
                destinationUrl = _urlProvider.GetFido2Url(destinationUrl, request.Type, request.Language);

            return new GetChallengeLinkResponse(challengeContext, destinationUrl.ToString(), nonce,
                _configuration.CacheExpirationTimeout);
        }
    }
}