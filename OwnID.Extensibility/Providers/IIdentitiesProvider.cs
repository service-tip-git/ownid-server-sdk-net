namespace OwnID.Extensibility.Providers
{
    public interface IIdentitiesProvider
    {
        /// <summary>
        ///     Generates new unique identifier
        /// </summary>
        public string GenerateContext();

        /// <summary>
        ///     Generates nonce
        /// </summary>
        public string GenerateNonce();

        /// <summary>
        ///     Generates user unique identifier
        /// </summary>
        public string GenerateUserId();

        /// <summary>
        ///     Generates magic link token
        /// </summary>
        public string GenerateMagicLinkToken();
    }
}