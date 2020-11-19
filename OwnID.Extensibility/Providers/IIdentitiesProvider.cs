namespace OwnID.Extensibility.Providers
{
    public interface IIdentitiesProvider
    {
        /// <summary>
        ///     Generates new unique identifier
        /// </summary>
        public string GenerateContext();

        /// <summary>
        ///     Verifies if provided <paramref name="context" /> is valid
        /// </summary>
        /// <param name="context">Challenge unique identifier</param>
        /// <returns>True if valid</returns>
        public bool IsContextFormatValid(string context);

        /// <summary>
        ///     Generates nonce
        /// </summary>
        public string GenerateNonce();

        /// <summary>
        ///     Generates user unique identifier
        /// </summary>
        public string GenerateUserId();
    }
}