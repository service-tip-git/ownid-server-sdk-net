using OwnID.Extensibility.Configuration;

namespace OwnID.Configuration
{
    /// <inheritdoc cref="ISmtpConfiguration"/>>
    public class SmtpConfiguration : ISmtpConfiguration
    {
        public string FromAddress { get; set; }
        
        public string FromName { get; set; }

        public string Host { get; set; }
        
        public int Port { get; set; }
        
        public bool UseSsl { get; set; }

        public string UserName { get; set; }

        public string Password { get; set; }
    }
}