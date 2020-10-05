namespace OwnIdSdk.NetCore3.Extensibility.Flow.Contracts.Internal
{
    public class StateRequest
    {
        public string RecoveryToken { get; set; }
        
        public string EncryptionToken { get; set; }
        
        public bool RequiresRecovery { get; set; }
    }
}