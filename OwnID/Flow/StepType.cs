namespace OwnID.Flow
{
    /// <summary>
    ///     Step type to show in Web App
    /// </summary>
    public enum StepType
    {
        // shared
        Starting,
        AcceptStart,
        ApprovePin,
        Declined,
        Success,
        Error,
        InternalConnectionRecovery,
        CheckUserExistence,
        EnterPasscode,
        ResetPasscode,
        Fido2Disclaimer,
        Stopped,

        Authorize,
        InstantAuthorize,
        Fido2Authorize,
        Link,
        Recover
    }
}