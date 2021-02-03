using OwnID.Extensibility.Flow.Contracts.Start;

namespace OwnID.Extensibility.Flow.Contracts
{
    public class OwnIdConnection
    {
        /// <summary>
        ///     User public key
        /// </summary>
        public string PublicKey { get; set; }

        /// <summary>
        ///     Fido2 signature counter
        /// </summary>
        public string Fido2SignatureCounter { get; set; }

        /// <summary>
        ///     Fido2 credential id
        /// </summary>
        public string Fido2CredentialId { get; set; }

        /// <summary>
        ///     Recovery token
        /// </summary>
        public string RecoveryToken { get; set; }

        /// <summary>
        ///     Recovery data. Encrypted
        /// </summary>
        public string RecoveryData { get; set; }
        
        /// <summary>
        /// Auth type
        /// </summary>
        public ConnectionAuthType AuthType { get; set; }
    }
}