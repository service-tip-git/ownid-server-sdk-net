namespace OwnID.Extensibility.Flow.Contracts
{
    public static class CookieNameTemplates
    {
        public const string PasswordlessRecovery = "psw-r-{0}";

        public const string PasswordlessEncryption = "psw-enc-{0}";
        
        public const string PasswordlessCredId = "psw-credid-{0}";
        
        public const string WebAppRecovery = "sign-r-{0}";
        
        public const string WebAppEncryption = "sign-enc-{0}";
    }
}