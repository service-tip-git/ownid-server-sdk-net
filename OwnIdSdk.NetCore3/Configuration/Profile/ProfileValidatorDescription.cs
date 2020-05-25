namespace OwnIdSdk.NetCore3.Configuration.Profile
{
    public class ProfileValidatorDescription
    {
        public ProfileValidatorDescription(string clientSideTypeNaming, string defaultErrorMessage)
        {
            ClientSideTypeNaming = clientSideTypeNaming;
            DefaultErrorMessage = defaultErrorMessage;
        }
        
        public string ClientSideTypeNaming { get; }
        
        public string DefaultErrorMessage { get; }
    }
}