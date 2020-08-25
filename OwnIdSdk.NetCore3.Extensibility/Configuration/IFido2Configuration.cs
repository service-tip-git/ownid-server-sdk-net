using System;

namespace OwnIdSdk.NetCore3.Extensibility.Configuration
{
    public interface IFido2Configuration
    {
        /// <summary>
        ///     Indicate if we use FIDO2
        /// </summary>
        bool Enabled { get; set; }
        
        /// <summary>
        ///     Fido2 origin
        /// </summary>
        Uri Origin { get; set; }
        
        /// <summary>
        ///     Fido 2 page
        /// </summary>
        Uri PasswordlessPageUrl { get; set; }
        
        /// <summary>
        ///     Fido2 relaying party id.
        /// </summary>
        /// <remarks>Must be the same at client application and the server side</remarks>
        string RelyingPartyId { get; set; }
        
        /// <summary>
        ///     Fido2 relaying party name
        /// </summary>
        /// <remarks>Must be the same at client application and the server side</remarks>
        string RelyingPartyName { get; set; }
        
        /// <summary>
        ///     Fido2 user display name
        /// </summary>
        string UserDisplayName { get; set; }
        
        /// <summary>
        ///     Fido2 user name
        /// </summary>
        string UserName { get; set; }
    }
}