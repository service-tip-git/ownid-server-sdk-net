namespace OwnID.Extensibility.Flow.Contracts.MagicLink
{
    public class ExchangeMagicLinkRequest
    {
        public string Context { get; set; }

        public string MagicToken { get; set; }

        public string CheckToken { get; set; }
    }
}