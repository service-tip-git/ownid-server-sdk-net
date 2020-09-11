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
        
        Fido2PartialRegister,
        Fido2PartialLogin,
        Fido2Link,
        Fido2LinkWithPin,
        Fido2Recover,
        Fido2RecoverWithPin
    }
}