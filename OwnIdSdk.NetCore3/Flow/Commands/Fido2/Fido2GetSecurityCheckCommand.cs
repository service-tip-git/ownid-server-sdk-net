using System.Threading.Tasks;
using Fido2NetLib;
using OwnIdSdk.NetCore3.Extensibility.Cache;
using OwnIdSdk.NetCore3.Extensibility.Configuration;
using OwnIdSdk.NetCore3.Extensibility.Flow;
using OwnIdSdk.NetCore3.Flow.Interfaces;
using OwnIdSdk.NetCore3.Flow.Steps;
using OwnIdSdk.NetCore3.Services;

namespace OwnIdSdk.NetCore3.Flow.Commands.Fido2
{
    public class Fido2GetSecurityCheckCommand : BaseFido2RegisterCommand
    {
        private readonly GetSecurityCheckCommand _getSecurityCheckCommand;

        public Fido2GetSecurityCheckCommand(IFido2 fido2, ICacheItemService cacheItemService, IJwtComposer jwtComposer,
            IFlowController flowController, IOwnIdCoreConfiguration configuration,
            GetSecurityCheckCommand getSecurityCheckCommand) : base(fido2, cacheItemService, jwtComposer,
            flowController, configuration)
        {
            _getSecurityCheckCommand = getSecurityCheckCommand;
        }

        protected override async Task<ICommandResult> GetResultAsync(ICommandInput input, CacheItem relatedItem,
            StepType currentStepType)
        {
            return await _getSecurityCheckCommand.ExecuteAsync(input, relatedItem, currentStepType, false);
        }
    }
}