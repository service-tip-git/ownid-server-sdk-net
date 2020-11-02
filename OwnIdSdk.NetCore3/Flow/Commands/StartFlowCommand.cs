using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using OwnIdSdk.NetCore3.Cryptography;
using OwnIdSdk.NetCore3.Extensibility.Cache;
using OwnIdSdk.NetCore3.Extensibility.Configuration;
using OwnIdSdk.NetCore3.Extensibility.Exceptions;
using OwnIdSdk.NetCore3.Extensibility.Flow;
using OwnIdSdk.NetCore3.Extensibility.Flow.Contracts;
using OwnIdSdk.NetCore3.Extensibility.Flow.Contracts.Jwt;
using OwnIdSdk.NetCore3.Extensibility.Json;
using OwnIdSdk.NetCore3.Extensions;
using OwnIdSdk.NetCore3.Flow.Commands.Authorize;
using OwnIdSdk.NetCore3.Flow.Commands.Recovery;
using OwnIdSdk.NetCore3.Flow.Steps;
using OwnIdSdk.NetCore3.Services;

namespace OwnIdSdk.NetCore3.Flow.Commands
{
    public class StartFlowCommand : BaseFlowCommand
    {
        private readonly ICacheItemService _cacheItemService;
        private readonly IOwnIdCoreConfiguration _configuration;
        private readonly IJwtService _jwtService;
        private readonly IServiceProvider _serviceProvider;

        public StartFlowCommand(ICacheItemService cacheItemService, IJwtService jwtService,
            IServiceProvider serviceProvider, IOwnIdCoreConfiguration configuration)
        {
            _cacheItemService = cacheItemService;
            _jwtService = jwtService;
            _serviceProvider = serviceProvider;
            _configuration = configuration;
        }

        protected override void Validate(ICommandInput input, CacheItem relatedItem)
        {
            if (relatedItem.HasFinalState)
                throw new CommandValidationException("Flow is already finished");

            if (!(input is CommandInput<string>))
                throw new InternalLogicException($"Incorrect input type for {nameof(StartFlowCommand)}");
        }

        protected override async Task<ICommandResult> ExecuteInternalAsync(ICommandInput input, CacheItem relatedItem,
            StepType currentStepType, bool isStateless)
        {
            var isFido2 = await SwitchToFido2FlowIfNeededAsync(((CommandInput<string>) input).Data, relatedItem);
            
            BaseFlowCommand command = relatedItem.FlowType switch
            {
                FlowType.Authorize => _serviceProvider.GetService<GetAuthProfileCommand>(),
                FlowType.PartialAuthorize => _serviceProvider.GetService<GetPartialInfoCommand>(),
                FlowType.Link => _serviceProvider.GetService<GetNextStepCommand>(),
                FlowType.Recover => _serviceProvider.GetService<RecoverAccountCommand>(),
                FlowType.RecoverWithPin => _serviceProvider.GetService<GetSecurityCheckCommand>(),
                FlowType.LinkWithPin => _serviceProvider.GetService<GetSecurityCheckCommand>(),
                FlowType.Fido2Register => _serviceProvider.GetService<GetPartialInfoCommand>(),
                FlowType.Fido2Login => _serviceProvider.GetService<GetPartialInfoCommand>(),
                FlowType.Fido2Link => _serviceProvider.GetService<GetPartialInfoCommand>(),
                FlowType.Fido2Recover => _serviceProvider.GetService<GetPartialInfoCommand>(),
                FlowType.Fido2LinkWithPin => _serviceProvider.GetService<GetSecurityCheckCommand>(),
                FlowType.Fido2RecoverWithPin => _serviceProvider.GetService<GetSecurityCheckCommand>(),
                _ => throw new InternalLogicException($"Not supported FlowType {relatedItem.FlowType}")
            };

            var commandResult = await command.ExecuteAsync(input, relatedItem, currentStepType, false, isFido2);

            if (!(commandResult is JwtContainer jwtContainer))
                throw new InternalLogicException("Incorrect command result type");

            await _cacheItemService.SetSecurityTokensAsync(relatedItem.Context, input.RequestToken,
                _jwtService.GetJwtHash(jwtContainer.Jwt).EncodeBase64String());

            return commandResult;
        }

        private async Task<bool> SwitchToFido2FlowIfNeededAsync(string requestBody, CacheItem cacheItem)
        {
            if (!_configuration.AuthenticationMode.IsFido2Enabled())
                return false;

            if (cacheItem.FlowType != FlowType.PartialAuthorize
                && cacheItem.FlowType != FlowType.Link
                && cacheItem.FlowType != FlowType.Recover
                && cacheItem.FlowType != FlowType.LinkWithPin
                && cacheItem.FlowType != FlowType.RecoverWithPin)
                return false;

            if (string.IsNullOrEmpty(requestBody))
                return false;

            var routing = OwnIdSerializer.Deserialize<ExtAuthenticationRouting>(requestBody);

            if (routing.Authenticator != ExtAuthenticatorType.Fido2)
                return false;

            var initialFlowType = cacheItem.FlowType;
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

            if (initialFlowType != cacheItem.FlowType)
                await _cacheItemService.UpdateFlowAsync(cacheItem.Context, cacheItem.FlowType,
                    cacheItem.ChallengeType);

            return true;
        }
    }
}