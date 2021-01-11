using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OwnID.Extensibility.Cache;
using OwnID.Extensibility.Configuration;
using OwnID.Extensibility.Exceptions;
using OwnID.Extensibility.Flow;
using OwnID.Extensibility.Flow.Contracts;
using OwnID.Extensibility.Json;
using OwnID.Extensibility.Metrics;
using OwnID.Extensibility.Providers;
using OwnID.Flow;
using OwnID.Flow.ResultActions;
using OwnID.Services;

namespace OwnID.Commands
{
    public class TrySwitchToFido2FlowCommand
    {
        private readonly ICacheItemRepository _cacheItemRepository;
        private readonly IOwnIdCoreConfiguration _coreConfiguration;
        private readonly IEventsMetricsService _eventsMetricsService;
        private readonly IUrlProvider _urlProvider;
        private readonly ILogger<TrySwitchToFido2FlowCommand> _logger;

        public TrySwitchToFido2FlowCommand(IOwnIdCoreConfiguration coreConfiguration,
            ICacheItemRepository cacheItemRepository, IUrlProvider urlProvider,
            ILogger<TrySwitchToFido2FlowCommand> logger, IEventsMetricsService eventsMetricsService = null)
        {
            _coreConfiguration = coreConfiguration;
            _cacheItemRepository = cacheItemRepository;
            _urlProvider = urlProvider;
            _logger = logger;
            _eventsMetricsService = eventsMetricsService;
        }

        public async Task<FrontendBehavior> ExecuteAsync(CacheItem cacheItem, string routingPayload)
        {
            if (!_coreConfiguration.Fido2.IsEnabled)
                return null;

            // If this is second attempt to call same start endpoint
            if (cacheItem.FlowType == FlowType.Fido2Login
                || cacheItem.FlowType == FlowType.Fido2Register
                || cacheItem.FlowType == FlowType.Fido2Link
                || cacheItem.FlowType == FlowType.Fido2LinkWithPin
                || cacheItem.FlowType == FlowType.Fido2Recover
                || cacheItem.FlowType == FlowType.Fido2RecoverWithPin)
                return null;

            // Not supported for Fido2 flows
            if (cacheItem.FlowType != FlowType.PartialAuthorize
                && cacheItem.FlowType != FlowType.Link
                && cacheItem.FlowType != FlowType.Recover
                && cacheItem.FlowType != FlowType.LinkWithPin
                && cacheItem.FlowType != FlowType.RecoverWithPin)
                return null;

            if (string.IsNullOrEmpty(routingPayload))
                return null;

            var routing = OwnIdSerializer.Deserialize<ExtAuthenticationRouting>(routingPayload);

            if (routing.Authenticator != ExtAuthenticatorType.Fido2)
                return null;

            if (!string.IsNullOrEmpty(routing.Error))
            {
                _logger.LogError($"Found error in FIDO2 ExtAuthenticationRouting object: {routing.Error}");
                return null;
            }
                
            var initialFlowType = cacheItem.FlowType;
            var initialChallengeType = cacheItem.ChallengeType;
            switch (routing.Type)
            {
                case "l":
                {
                    if (cacheItem.FlowType == FlowType.PartialAuthorize)
                        cacheItem.FlowType = FlowType.Fido2Login;
                    break;
                }
                case "r":
                    cacheItem.FlowType = cacheItem.FlowType switch
                    {
                        FlowType.PartialAuthorize => FlowType.Fido2Register,
                        FlowType.Link => FlowType.Fido2Link,
                        FlowType.LinkWithPin => FlowType.Fido2LinkWithPin,
                        FlowType.Recover => FlowType.Fido2Recover,
                        FlowType.RecoverWithPin => FlowType.Fido2RecoverWithPin,
                        _ => cacheItem.FlowType
                    };

                    if (cacheItem.FlowType == FlowType.Fido2Register)
                        cacheItem.ChallengeType = ChallengeType.Register;
                    break;
                default:
                    throw new InternalLogicException($"Incorrect fido2 request: '{routing.Type}'");
            }

            if (initialFlowType == cacheItem.FlowType)
                return null;

            await _cacheItemRepository.UpdateAsync(cacheItem.Context, item =>
            {
                item.FlowType = cacheItem.FlowType;
                item.ChallengeType = cacheItem.ChallengeType;
            });

            if (_eventsMetricsService != null && initialChallengeType != cacheItem.ChallengeType)
            {
                _eventsMetricsService?.LogSwitchAsync(initialChallengeType.ToEventType());
                _eventsMetricsService?.LogStartAsync(cacheItem.ChallengeType.ToEventType());
            }

            // TODO: rework to exclude explicit url creation 
            return new FrontendBehavior(StepType.Fido2Authorize, cacheItem.ChallengeType,
                new CallAction(_urlProvider.GetChallengeUrl(cacheItem.Context, cacheItem.ChallengeType, "/fido2")));
        }
    }
}