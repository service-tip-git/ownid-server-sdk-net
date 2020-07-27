using OwnIdSdk.NetCore3.Extensibility.Cache;
using OwnIdSdk.NetCore3.Extensibility.Flow;
using OwnIdSdk.NetCore3.Flow.Steps;

namespace OwnIdSdk.NetCore3.Flow.Interfaces
{
    public interface IFlowController
    {
        FrontendBehavior GetExpectedFrontendBehavior(CacheItem cacheItem, StepType currentStep);

        IStep GetStep(FlowType flowType, StepType currentStep);
    }
}