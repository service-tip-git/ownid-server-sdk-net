namespace OwnID.Extensibility.Flow.Contracts.Start
{
    public class AcceptStartRequest
    {
        public bool SupportsFido2 { get; set; }
        
        public ConnectionAuthType? AuthType { get; set; }
        
        public string ExtAuthPayload { get; set; }
    }
}