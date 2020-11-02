namespace OwnIdSdk.NetCore3.Extensibility.Flow
{
    public enum FlowType
    {
        Authorize,
        PartialAuthorize,
        Link,
        LinkWithPin,
        Recover,
        RecoverWithPin,

        Fido2Register,
        Fido2Login,
        Fido2Link,
        Fido2LinkWithPin,
        Fido2Recover,
        Fido2RecoverWithPin
    }
}