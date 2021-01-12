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
        [GigyaFields("data.ownIdConnections")]
        Connections = 2,
        [GigyaFields("data.ownIdConnections.pubKey")]
        ConnectionPublicKeys = 4,
        [GigyaFields("data.userSettings")]
        Settings = 8,

        Default = UID | Connections,
    }
}