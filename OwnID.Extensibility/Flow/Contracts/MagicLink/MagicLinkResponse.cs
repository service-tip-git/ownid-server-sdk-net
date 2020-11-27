namespace OwnID.Extensibility.Flow.Contracts.MagicLink
{
    public class MagicLinkResponse
    {
        public string CheckTokenKey { get; set; }

        public string CheckTokenValue { get; set; }

        public uint CheckTokenLifetime { get; set; }
    }
}