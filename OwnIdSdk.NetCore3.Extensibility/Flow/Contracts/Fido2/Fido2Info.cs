namespace OwnIdSdk.NetCore3.Extensibility.Flow.Contracts.Fido2
{
    public class Fido2Info
    {
        public string PublicKey { get; set; }

        public uint SignatureCounter { get; set; }

        public string CredentialId { get; set; }

        public string UserId { get; set; }
    }
}