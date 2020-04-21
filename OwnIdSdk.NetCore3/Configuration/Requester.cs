namespace OwnIdSdk.NetCore3.Configuration
{
    public class Requester
    {
        public string DID { get; set; }
        
        public string PublicKey { get; set; }
        
        //  can be substituted by organization?
        public string Name { get; set; }
    }
}