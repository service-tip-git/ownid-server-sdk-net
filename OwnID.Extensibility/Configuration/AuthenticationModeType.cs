namespace OwnID.Extensibility.Configuration
{
    public enum AuthenticationModeType
    {
        /// <summary>
        ///     Enforce to use only OwnID authenticator
        /// </summary>
        OwnIdOnly,

        /// <summary>
        ///     Enforce to use only FIDO2 authenticator
        /// </summary>
        Fido2Only,

        /// <summary>
        ///     All possible Authentication Modes
        /// </summary>
        /// <remarks>
        ///     FIDO2 has higher priority
        /// </remarks>
        All
    }
    
    public static class AuthenticationModeTypeExtensions
    {
        public static bool IsFido2Enabled(this AuthenticationModeType mode)
        {
            return mode == AuthenticationModeType.All || mode == AuthenticationModeType.Fido2Only;
        }
    }
}