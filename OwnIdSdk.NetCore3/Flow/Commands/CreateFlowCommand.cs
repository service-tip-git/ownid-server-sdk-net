using System;
using System.Threading.Tasks;
using OwnIdSdk.NetCore3.Extensibility.Configuration;
using OwnIdSdk.NetCore3.Extensibility.Flow;
using OwnIdSdk.NetCore3.Extensibility.Flow.Abstractions;
using OwnIdSdk.NetCore3.Extensibility.Flow.Contracts;
using OwnIdSdk.NetCore3.Extensibility.Providers;
using OwnIdSdk.NetCore3.Services;

namespace OwnIdSdk.NetCore3.Flow.Commands
{
    public class CreateFlowCommand
    {
        private readonly ICacheItemService _cacheItemService;
        private readonly IOwnIdCoreConfiguration _coreConfiguration;
        private readonly IIdentitiesProvider _identitiesProvider;
        private readonly IAccountLinkHandler _linkHandler;
        private readonly IUrlProvider _urlProvider;

        public CreateFlowCommand(ICacheItemService cacheItemService, IUrlProvider urlProvider,
            IIdentitiesProvider identitiesProvider, IOwnIdCoreConfiguration coreConfiguration,
            IAccountLinkHandler linkHandler = null)
        {
            _cacheItemService = cacheItemService;
            _urlProvider = urlProvider;
            _identitiesProvider = identitiesProvider;
            _coreConfiguration = coreConfiguration;
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

                    if (state.ConnectedDevicesCount >= _coreConfiguration.MaximumNumberOfConnectedDevices)
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

            if (_coreConfiguration.Fido2.Enabled
                && (
                    flowType == FlowType.PartialAuthorize
                    || request.Type == ChallengeType.Link
                    || request.Type == ChallengeType.Recover
                ))
                destinationUrl = _urlProvider.GetFido2Url(destinationUrl, request.Type);

            return new GetChallengeLinkResponse(
                challengeContext,
                destinationUrl.ToString(),
                nonce,
                _coreConfiguration.CacheExpirationTimeout);
        }
    }
}