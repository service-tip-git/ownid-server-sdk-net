using System.Threading.Tasks;
using OwnIdSdk.NetCore3.Cryptography;
using OwnIdSdk.NetCore3.Extensibility.Cache;
using OwnIdSdk.NetCore3.Extensibility.Exceptions;
using OwnIdSdk.NetCore3.Extensibility.Flow;
using OwnIdSdk.NetCore3.Extensibility.Flow.Abstractions;
using OwnIdSdk.NetCore3.Extensibility.Flow.Contracts.Jwt;
using OwnIdSdk.NetCore3.Extensibility.Providers;
using OwnIdSdk.NetCore3.Extensions;
using OwnIdSdk.NetCore3.Flow.Adapters;
using OwnIdSdk.NetCore3.Flow.Commands.Authorize;
using OwnIdSdk.NetCore3.Flow.Commands.Link;
using OwnIdSdk.NetCore3.Flow.Commands.Recovery;
using OwnIdSdk.NetCore3.Flow.Interfaces;
using OwnIdSdk.NetCore3.Services;

namespace OwnIdSdk.NetCore3.Flow.Commands
{
    public class StartFlowFlowCommand : BaseFlowCommand
    {
        private readonly ICacheItemService _cacheItemService;
        private readonly IFlowController _flowController;
        private readonly IIdentitiesProvider _identitiesProvider;
        private readonly IJwtComposer _jwtComposer;
        private readonly IJwtService _jwtService;
        private readonly IAccountLinkHandlerAdapter _linkHandlerAdapter;
        private readonly IAccountRecoveryHandler _recoveryHandler;

        public StartFlowFlowCommand(ICacheItemService cacheItemService, IJwtService jwtService,
            IJwtComposer jwtComposer, IFlowController flowController, IIdentitiesProvider identitiesProvider,
            IAccountLinkHandlerAdapter linkHandlerAdapter = null, IAccountRecoveryHandler recoveryHandler = null)
        {
            _cacheItemService = cacheItemService;
            _jwtService = jwtService;
            _jwtComposer = jwtComposer;
            _flowController = flowController;
            _identitiesProvider = identitiesProvider;
            _linkHandlerAdapter = linkHandlerAdapter;
            _recoveryHandler = recoveryHandler;
        }

        protected override void Validate()
        {
            // TODO: add flag related validation
        }

        protected override async Task<ICommandResult> ExecuteInternal(ICommandInput input, CacheItem relatedItem,
            StepType currentStepType)
        {
            var cacheItem = await _cacheItemService.GetCacheItemByContextAsync(input.Context);

            if (cacheItem.HasFinalState)
                throw new CommandValidationException("Flow is already finished");
            
            BaseFlowCommand command;

            switch (cacheItem.FlowType)
            {
                case FlowType.Authorize:
                    command = new GetAuthProfileCommand(_jwtComposer, _flowController, _identitiesProvider);
                    break;
                case FlowType.PartialAuthorize:
                    command = new GetPartialInfoCommand(_jwtComposer, _flowController, _identitiesProvider);
                    break;
                case FlowType.Link:
                    command = new GetLinkProfileCommand(_jwtComposer, _flowController, _linkHandlerAdapter);
                    break;
                case FlowType.Recover:
                    command = new RecoverAccountCommand(_jwtComposer, _flowController, _recoveryHandler);
                    break;
                case FlowType.RecoverWithPin:
                case FlowType.LinkWithPin:
                    command = new GetSecurityCheckCommand(_cacheItemService, _jwtComposer, _flowController);
                    break;
                default:
                    throw new InternalLogicException($"Not supported FlowType {cacheItem.FlowType}");
            }

            var commandResult = await command.ExecuteAsync(input, relatedItem, currentStepType);

            if (!(commandResult is JwtContainer jwtContainer))
                throw new InternalLogicException("Incorrect command result type");

            await _cacheItemService.SetSecurityTokensAsync(cacheItem.Context, input.RequestToken,
                _jwtService.GetJwtHash(jwtContainer.Jwt).GetUrlEncodeString());

            return commandResult;
        }
    }
}