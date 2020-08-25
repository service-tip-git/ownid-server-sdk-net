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

        Authorize,
        InstantAuthorize,
        Link,
        Recover,
        
        Fido2Register,
        Fido2Login,
        Fido2Success,
    }
}