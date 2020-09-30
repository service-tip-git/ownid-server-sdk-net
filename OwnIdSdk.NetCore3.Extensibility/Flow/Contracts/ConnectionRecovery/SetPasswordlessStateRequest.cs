namespace OwnIdSdk.NetCore3.Extensibility.Flow.Contracts.ConnectionRecovery
{
    public class SetPasswordlessStateRequest
    {
        public string RecoveryToken { get; set; }
        
        public string EncryptionToken { get; set; }
        
        public bool RequiresRecovery { get; set; }
    }
}