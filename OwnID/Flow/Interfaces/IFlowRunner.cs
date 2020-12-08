using System.Threading.Tasks;
using OwnID.Flow.Commands;
using OwnID.Flow.Steps;
using OwnID.Extensibility.Flow;

namespace OwnID.Flow.Interfaces
{
    public interface IFlowRunner
    {
        Task<ICommandResult> RunAsync(ICommandInput input, StepType currentStep);
    }
}