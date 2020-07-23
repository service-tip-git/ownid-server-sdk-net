using System;
using System.Threading.Tasks;
using OwnIdSdk.NetCore3.Extensibility.Exceptions;
using OwnIdSdk.NetCore3.Extensibility.Flow;
using OwnIdSdk.NetCore3.Flow.Commands;
using OwnIdSdk.NetCore3.Flow.Interfaces;
using OwnIdSdk.NetCore3.Flow.Steps;
using OwnIdSdk.NetCore3.Services;

namespace OwnIdSdk.NetCore3.Flow
{
    public class FlowRunner : IFlowRunner
    {
        private readonly ICacheItemService _cacheItemService;
        private readonly IFlowController _flowController;
        private readonly IServiceProvider _serviceProvider;

        public FlowRunner(IFlowController flowController, ICacheItemService cacheItemService,
            IServiceProvider serviceProvider)
        {
            _flowController = flowController;
            _cacheItemService = cacheItemService;
            _serviceProvider = serviceProvider;
        }

        public async Task<ICommandResult> RunAsync(ICommandInput input, StepType currentStep)
        {
            var item = await _cacheItemService.GetCacheItemByContextAsync(input.Context);
            var commandType = _flowController.GetStep(item.FlowType, currentStep).GetRelatedCommandType();

            if (!(_serviceProvider.GetService(commandType) is BaseFlowCommand command))
                throw new InternalLogicException("Can not inject command");

            return await command.ExecuteAsync(input, item, currentStep);
        }
    }
}