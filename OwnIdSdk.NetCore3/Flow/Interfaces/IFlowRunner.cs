using System.Threading.Tasks;
using OwnIdSdk.NetCore3.Extensibility.Flow;
using OwnIdSdk.NetCore3.Flow.Commands;
using OwnIdSdk.NetCore3.Flow.Steps;

namespace OwnIdSdk.NetCore3.Flow.Interfaces
{
    public interface IFlowRunner
    {
        Task<ICommandResult> RunAsync(ICommandInput input, StepType currentStep);
    }
}