namespace OwnID.Server.WebApp
{
    /// <summary>
    ///     WebApp options
    /// </summary>
    public class WebAppOptions
    {
        public const string ConfigurationName = "WebApp";

        /// <summary>
        ///     Is development environment
        /// </summary>
        public bool IsDevEnvironment { get; set; }

        /// <summary>
        ///     Cookies expiration in days
        /// </summary>
        /// <remarks>Default value is 5 years</remarks>
        public int CookieExpiration { get; set; } = 365 * 5;

        /// <summary>
        ///     Top domain
        /// </summary>
        /// <remarks>Default value is "ownid.com"</remarks>
        public string TopDomain { get; set; } = "ownid.com";

        public static WebAppOptions Default => new WebAppOptions();
    }
}