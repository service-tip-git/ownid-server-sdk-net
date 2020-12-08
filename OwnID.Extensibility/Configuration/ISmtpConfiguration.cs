namespace OwnID.Extensibility.Configuration
{
    public interface ISmtpConfiguration
    {
        /// <summary>
        ///     From email address
        /// </summary>
        public string FromAddress { get; set; }

        /// <summary>
        ///     From user friendly address
        /// </summary>
        public string FromName { get; set; }

        /// <summary>
        ///     SMTP server host
        /// </summary>
        public string Host { get; set; }

        /// <summary>
        ///     SMTP server port
        /// </summary>
        /// <remarks>
        ///     Default value 465
        /// </remarks>
        public int Port { get; set; }

        /// <summary>
        ///     Use secure transport
        /// </summary>
        public bool UseSsl { get; set; }

        /// <summary>
        ///     Auth. user name
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        ///     Auth. password
        /// </summary>
        public string Password { get; set; }
    }
}