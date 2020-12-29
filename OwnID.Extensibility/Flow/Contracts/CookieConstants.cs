namespace OwnID.Extensibility.Flow.Contracts
{
    public static class CookieNameTemplates
    {
        public const string CredId = "sign-credid-{0}";
        
        public const string Recovery = "sign-r-{0}";
        
        public const string Encryption = "sign-enc-{0}";
    }

    public static class CookieValuesConstants
    {
        public const string SubValueSeparator = ":::";

        public const string PasscodeEnding = SubValueSeparator + "passcode";
    }
}