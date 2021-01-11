using System.Threading.Tasks;
using OwnID.Extensibility.Flow.Contracts;

namespace OwnID.Flow.Interfaces
{
    public interface IFlowRunner
    {
        Task<ITransitionResult> RunAsync(ITransitionInput input, StepType currentStep);
    }
}