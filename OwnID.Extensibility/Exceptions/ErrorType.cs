namespace OwnID.Extensibility.Exceptions
{
    /// <summary>
    ///     OwnId Error
    /// </summary>
    public enum ErrorType
    {
        /// <summary>
        ///     User not found
        /// </summary>
        UserAlreadyExists,
        
        /// <summary>
        ///     Recover token expired
        /// </summary>
        RecoveryTokenExpired
    }
}