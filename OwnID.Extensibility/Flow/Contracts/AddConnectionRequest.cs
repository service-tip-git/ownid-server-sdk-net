namespace OwnID.Extensibility.Flow.Contracts
{
    public class AddConnectionRequest : GetStatusRequest
    {
        public string DID { get; set; }
    }
}