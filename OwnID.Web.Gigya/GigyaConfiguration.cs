namespace OwnID.Web.Gigya
{
    public class GigyaConfiguration
    {
        public string DataCenter { get; set; }

        public string ApiKey { get; set; }

        public string UserKey { get; set; }

        public string SecretKey { get; set; }

        public GigyaLoginType LoginType { get; set; }
    }
}