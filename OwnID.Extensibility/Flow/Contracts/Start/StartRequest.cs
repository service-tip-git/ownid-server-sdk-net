namespace OwnID.Extensibility.Flow.Contracts.Start
{
    public class StartRequest
    {
        public string RecoveryToken { get; set; }

        public string EncryptionToken { get; set; }

        public string CredId { get; set; }
    }
}