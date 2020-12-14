namespace OwnID.Extensibility.Configuration
{
    /// <summary>
    /// The way how application should handle unavailability of FIDO2
    /// </summary>
    public enum Fido2FallbackBehavior
    {
        /// <summary>
        ///     Fallback to OwnID app with 4 digits user code
        /// </summary>
        Passcode,

        /// <summary>
        ///      Fallback to OwnID app only
        /// </summary>
        Basic,

        /// <summary>
        ///     Show an error and block any other actions
        /// </summary>
        Block
    }
}