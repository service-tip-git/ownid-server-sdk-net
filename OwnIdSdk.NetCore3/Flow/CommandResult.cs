using OwnIdSdk.NetCore3.Extensibility.Flow;

namespace OwnIdSdk.NetCore3.Flow
{
    public class CommandResult<T> : ICommandResult
    {
        public T Data { get; set; }
    }
}