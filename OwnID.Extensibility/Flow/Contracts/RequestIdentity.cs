namespace OwnID.Extensibility.Flow.Contracts
{
    public class RequestIdentity
    {
        public string Context { get; set; }

        public string RequestToken { get; set; }

        public string ResponseToken { get; set; }
    }
}