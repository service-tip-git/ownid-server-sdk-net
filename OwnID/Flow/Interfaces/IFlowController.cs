using OwnID.Flow.Steps;
using OwnID.Extensibility.Cache;
using OwnID.Extensibility.Flow;

namespace OwnID.Flow.Interfaces
{
    public interface IFlowController
    {
        FrontendBehavior GetExpectedFrontendBehavior(CacheItem cacheItem, StepType currentStep);

        IStep GetStep(FlowType flowType, StepType currentStep);
    }
}