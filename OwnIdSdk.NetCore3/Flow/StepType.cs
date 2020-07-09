namespace OwnIdSdk.NetCore3.Flow
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
        Link,
        Recover
    }
}