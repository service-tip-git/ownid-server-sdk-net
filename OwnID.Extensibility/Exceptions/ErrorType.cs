namespace OwnID.Extensibility.Exceptions
{
    /// <summary>
    ///     OwnId Error
    /// </summary>
    public enum ErrorType
    {
        /// <summary>
        ///     User already exits
        /// </summary>
        UserAlreadyExists,
        
        /// <summary>
        ///     User not found
        /// </summary>
        UserNotFound,
        
        /// <summary>
        ///     Recover token expired
        /// </summary>
        RecoveryTokenExpired,
        
        /// <summary>
        ///     Requires FIDO2
        /// </summary>
        RequiresBiometricInput,
    }
}