namespace OwnIdSdk.NetCore3.Extensibility.Flow.Contracts.Fido2
{
    public class InitFido2Request
    {
        public bool IsIncompatible { get; set; }
        
        public string CredId { get; set; }
    }
}