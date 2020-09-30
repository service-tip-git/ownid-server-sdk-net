namespace OwnIdSdk.NetCore3.Extensibility.Flow.Contracts.ConnectionRecovery
{
    public class PasswordlessStateResponse
    {
        public string RecoveryToken { get; set; }
        
        public string EncryptionToken { get; set; }
    }
}