namespace OwnID.Extensibility.Flow.Contracts
{
    public class UserExistsRequest
    {
        public ExtAuthenticatorType? AuthenticatorType { get; set; }
        
        public string UserIdentifier { get; set; }
        
        public bool ErrorOnExisting { get; set; }
        
        public bool ErrorOnNoEntry { get; set; }
    }
}