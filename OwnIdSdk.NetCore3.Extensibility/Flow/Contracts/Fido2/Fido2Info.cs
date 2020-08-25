namespace OwnIdSdk.NetCore3.Extensibility.Flow.Contracts.Fido2
{
    public class Fido2Info
    {
        public string PublickKey { get; set; }
        
        public uint SignatureCounter { get; set; }
        
        public string CredentialId { get; set; }
    }
}