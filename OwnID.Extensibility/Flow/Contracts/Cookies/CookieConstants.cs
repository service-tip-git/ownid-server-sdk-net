namespace OwnID.Extensibility.Flow.Contracts.Cookies
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

        public const string ValueLatestVersion = "1";

        /// <summary>
        /// <![CDATA[<version>:::<type>:::]]>
        /// should also add value like this <![CDATA[<value>:::<valueN>]]> 
        /// </summary>
        public const string ValueStarting = "{0}" + SubValueSeparator + "{1}" + SubValueSeparator;

        public const string Fido2CookieType = "fido2";
        
        public const string BasicCookieType = "basic";
        
        public const string PasscodeCookieType = "passcode";
        
        public const string RecoveryCookieType = "recovery";
    }
}