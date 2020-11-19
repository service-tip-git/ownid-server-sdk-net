using OwnID.Extensibility.Flow;

namespace OwnID.Flow
{
    public class CommandResult<T> : ICommandResult
    {
        public T Data { get; set; }
    }
}