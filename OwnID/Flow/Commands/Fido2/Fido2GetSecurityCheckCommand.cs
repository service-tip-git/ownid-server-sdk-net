using System.Threading.Tasks;
using Fido2NetLib;
using OwnID.Flow.Interfaces;
using OwnID.Flow.Steps;
using OwnID.Services;
using OwnID.Extensibility.Cache;
using OwnID.Extensibility.Configuration;
using OwnID.Extensibility.Flow;
using OwnID.Extensibility.Providers;

namespace OwnID.Flow.Commands.Fido2
{
    public class Fido2GetSecurityCheckCommand : BaseFido2RegisterCommand
    {
        private readonly GetSecurityCheckCommand _getSecurityCheckCommand;

        public Fido2GetSecurityCheckCommand(IFido2 fido2, ICacheItemService cacheItemService, IJwtComposer jwtComposer,
            IFlowController flowController, IOwnIdCoreConfiguration configuration,
            IIdentitiesProvider identitiesProvider, IEncodingService encodingService,
            GetSecurityCheckCommand getSecurityCheckCommand) : base(fido2, cacheItemService,
            jwtComposer, flowController, configuration, identitiesProvider, encodingService)
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