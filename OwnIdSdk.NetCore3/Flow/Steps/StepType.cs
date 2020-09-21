namespace OwnIdSdk.NetCore3.Flow.Steps
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
        InternalConnectionRecovery,

        Authorize,
        InstantAuthorize,
        Link,
        Recover,
        
        Fido2Success
    }
}