namespace OwnID.Extensibility.Flow.Contracts.Fido2
{
    public class InitFido2Response
    {
        public bool UserExists { get; set; }
        
        public ClientSideFido2Config Config { get; set; }
        
        public bool IsFido2Only { get; set; }
        
        public string CredId { get; set; }

        public class ClientSideFido2Config
        {
            public string RelyingPartyId { get; set; }

            public string RelyingPartyName { get; set; }

            public string UserDisplayName { get; set; }

            public string UserName { get; set; }
        }
    }
}