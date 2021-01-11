namespace OwnID.Extensibility.Flow.Contracts
{
    public class UserIdentification
    {
        public ExtAuthenticatorType? AuthenticatorType { get; set; }
        
        public string UserIdentifier { get; set; }
    }
}