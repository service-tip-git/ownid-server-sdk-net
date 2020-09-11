using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using OwnIdSdk.NetCore3.Cryptography;
using OwnIdSdk.NetCore3.Extensibility.Cache;
using OwnIdSdk.NetCore3.Extensibility.Configuration;
using OwnIdSdk.NetCore3.Extensibility.Exceptions;
using OwnIdSdk.NetCore3.Extensibility.Flow;
using OwnIdSdk.NetCore3.Extensibility.Flow.Contracts.Jwt;
using OwnIdSdk.NetCore3.Extensions;
using OwnIdSdk.NetCore3.Flow.Commands.Authorize;
using OwnIdSdk.NetCore3.Flow.Commands.Fido2;
using OwnIdSdk.NetCore3.Flow.Commands.Recovery;
using OwnIdSdk.NetCore3.Flow.Steps;
using OwnIdSdk.NetCore3.Services;

namespace OwnIdSdk.NetCore3.Flow.Commands
{
    public class StartFlowCommand : BaseFlowCommand
    {
        private readonly ICacheItemService _cacheItemService;
        private readonly IJwtService _jwtService;
        private readonly IServiceProvider _serviceProvider;
        private readonly IOwnIdCoreConfiguration _configuration;

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

            if (!(input is CommandInput<string> request))
                throw new InternalLogicException($"Incorrect input type for {nameof(StartFlowCommand)}");

            SwitchToFido2FlowIfNeededAsync(request.Data, relatedItem).Wait();
        }

        protected override async Task<ICommandResult> ExecuteInternal(ICommandInput input, CacheItem relatedItem,
            StepType currentStepType)
        {
            BaseFlowCommand command = relatedItem.FlowType switch
            {
                FlowType.Authorize => _serviceProvider.GetService<GetAuthProfileCommand>(),
                FlowType.PartialAuthorize => _serviceProvider.GetService<GetPartialInfoCommand>(),
                FlowType.Link => _serviceProvider.GetService<GetNextStepCommand>(),
                FlowType.Recover => _serviceProvider.GetService<RecoverAccountCommand>(),
                FlowType.RecoverWithPin => _serviceProvider.GetService<GetSecurityCheckCommand>(),
                FlowType.LinkWithPin => _serviceProvider.GetService<GetSecurityCheckCommand>(),
                FlowType.Fido2PartialRegister => _serviceProvider.GetService<Fido2RegisterCommand>(),
                FlowType.Fido2PartialLogin => _serviceProvider.GetService<Fido2LoginCommand>(),
                FlowType.Fido2Link => _serviceProvider.GetService<Fido2LinkCommand>(),
                FlowType.Fido2LinkWithPin => _serviceProvider.GetService<Fido2GetSecurityCheckCommand>(),
                FlowType.Fido2Recover => _serviceProvider.GetService<Fido2RecoverCommand>(),
                FlowType.Fido2RecoverWithPin => _serviceProvider.GetService<Fido2GetSecurityCheckCommand>(),
                _ => throw new InternalLogicException($"Not supported FlowType {relatedItem.FlowType}")
            };

            var commandResult = await command.ExecuteAsync(input, relatedItem, currentStepType, false);

            if (!(commandResult is JwtContainer jwtContainer))
                throw new InternalLogicException("Incorrect command result type");

            await _cacheItemService.SetSecurityTokensAsync(relatedItem.Context, input.RequestToken,
                _jwtService.GetJwtHash(jwtContainer.Jwt).GetUrlEncodeString());

            return commandResult;
        }

        private async Task SwitchToFido2FlowIfNeededAsync(string requestBody, CacheItem cacheItem)
        {
            if (!_configuration.Fido2.Enabled)
                return;

            if (cacheItem.FlowType != FlowType.PartialAuthorize
                && cacheItem.FlowType != FlowType.Link
                && cacheItem.FlowType != FlowType.Recover
                && cacheItem.FlowType != FlowType.LinkWithPin
                && cacheItem.FlowType != FlowType.RecoverWithPin)
            {
                return;
            }

            if (string.IsNullOrEmpty(requestBody))
                return;

            JsonDocument json;
            try
            {
                json = JsonDocument.Parse(requestBody);
            }
            catch
            {
                return;
            }

            if (json == null
                || !json.RootElement.TryGetProperty("fido2", out var fido2Element)
                || !fido2Element.TryGetProperty("type", out var typeElement))
            {
                return;
            }

            var initialFlowType = cacheItem.FlowType;
            var fido2RequestType = typeElement.GetString();
            switch (fido2RequestType)
            {
                case "l":
                {
                    if (cacheItem.FlowType == FlowType.PartialAuthorize)
                        cacheItem.FlowType = FlowType.Fido2PartialLogin;
                    break;
                }
                case "r":
                    cacheItem.FlowType = cacheItem.FlowType switch
                    {
                        FlowType.PartialAuthorize => FlowType.Fido2PartialRegister,
                        FlowType.Link => FlowType.Fido2Link,
                        FlowType.LinkWithPin => FlowType.Fido2LinkWithPin,
                        FlowType.Recover => FlowType.Fido2Recover,
                        FlowType.RecoverWithPin => FlowType.Fido2RecoverWithPin,
                        _ => cacheItem.FlowType
                    };
                    break;
                default:
                    throw new InternalLogicException($"Incorrect fido 2 request: '{fido2RequestType}'");
            }

            if (initialFlowType != cacheItem.FlowType)
            {
                await _cacheItemService.UpdateFlowAsync(cacheItem.Context, cacheItem.FlowType);
            }
        }
    }
}