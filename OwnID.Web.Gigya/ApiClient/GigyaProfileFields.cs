using System;

namespace OwnID.Web.Gigya.ApiClient
{
    /// <summary>
    ///     Gigya profile fields available for querying
    /// </summary>
    [Flags]
    public enum GigyaProfileFields
    {
        [GigyaFields("UID")]
        UID = 1,
        [GigyaFields("data.ownId.connections")]
        Connections = 2,
        [GigyaFields("data.ownId.connections.pubKey")]
        ConnectionPublicKeys = 4,
        [GigyaFields("data.ownId.settings")]
        Settings = 8,
    }
}