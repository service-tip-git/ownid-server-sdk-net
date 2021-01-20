namespace OwnID.Extensibility.Flow.Contracts
{
    public class AddConnectionRequest : GetStatusRequest
    {
        public string Payload { get; set; }
    }
}