using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using OwnID.Extensibility.Configuration;
using OwnID.Extensibility.Flow;
using OwnID.Extensibility.Flow.Contracts;
using OwnID.Flow.Interfaces;
using OwnID.Flow.Setups;
using OwnID.Flow.Setups.Fido2;
using OwnID.Flow.Setups.Partial;
using OwnID.Services;

namespace OwnID.Flow
{
    public class FlowRunner : IFlowRunner
    {
        private readonly ICacheItemRepository _cacheItemRepository;
        private readonly IServiceProvider _serviceProvider;

        public FlowRunner(ICacheItemRepository cacheItemRepository, IServiceProvider serviceProvider, IOwnIdCoreConfiguration coreConfiguration)
        {
            _cacheItemRepository = cacheItemRepository;
            _serviceProvider = serviceProvider;

            Flows = new Dictionary<FlowType, BaseFlow>();
            // TODO: check features for flows
            AddFlow<PartialAuthorizeFlow>();
            AddFlow<LinkFlow>();
            AddFlow<LinkWithPinFlow>();
            AddFlow<RecoveryFlow>();
            AddFlow<RecoveryWithPinFlow>();

            if (coreConfiguration.Fido2.IsEnabled)
            {
                AddFlow<Fido2RegisterFlow>();
                AddFlow<Fido2LoginFlow>();
                AddFlow<Fido2LinkFlow>();
                AddFlow<Fido2RecoveryFlow>();
                AddFlow<Fido2LinkFlow>(FlowType.Fido2LinkWithPin);
                AddFlow<Fido2RecoveryFlow>(FlowType.Fido2RecoverWithPin);
            }
        }

        private Dictionary<FlowType, BaseFlow> Flows { get; }

        // TODO: change return type
        public async Task<ITransitionResult> RunAsync(ITransitionInput input, StepType currentStep)
        {
            var item = await _cacheItemRepository.GetAsync(input.Context);
            input.IsDesktop = item.IsDesktop;
            return await Flows[item.FlowType].RunAsync(input, currentStep, item);
        }

        private void AddFlow<TFlow>(FlowType? flowType  = null) where TFlow : BaseFlow
        {
            var flow = _serviceProvider.GetService<TFlow>();
            Flows.TryAdd(flowType ?? flow.Type, flow);
        }
    }
}