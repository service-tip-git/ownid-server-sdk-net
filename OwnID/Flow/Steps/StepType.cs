namespace OwnID.Flow.Steps
{
    /// <summary>
    ///     Step type to show in Web App
    /// </summary>
    public enum StepType
    {
        // shared
        Starting,
        ApprovePin,
        Declined,
        Success,
        Error,
        InternalConnectionRecovery,
        CheckUserExistence,
        EnterPasscode,
        ResetPasscode,

        Authorize,
        InstantAuthorize,
        Fido2Authorize,
        Link,
        Recover
    }
}