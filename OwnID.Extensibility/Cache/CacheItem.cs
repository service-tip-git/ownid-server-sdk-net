using System;
using System.Diagnostics;
using OwnID.Extensibility.Flow;

namespace OwnID.Extensibility.Cache
{
    /// <summary>
    ///     OwnID flow detection fields store structure
    /// </summary>
    [DebuggerDisplay("{Context}: {Status} (ChallengeType: {ChallengeType}/FlowType: {FlowType})")]
    public class CacheItem : ICloneable
    {
        /// <summary>
        ///     OwnID flow unique identifier
        /// </summary>
        public string Context { get; set; }

        /// <summary>
        ///     State
        /// </summary>
        public CacheItemStatus Status { get; set; } = CacheItemStatus.Initiated;

        /// <summary>
        ///     Nonce
        /// </summary>
        public string Nonce { get; set; }

        /// <summary>
        ///     User unique identity
        /// </summary>
        public string DID { get; set; }

        /// <summary>
        ///     Challenge type related to the <c>Context</c> and <see cref="Nonce" />
        /// </summary>
        public ChallengeType ChallengeType { get; set; }

        /// <summary>
        ///     <see cref="Flow.FlowType" /> that should be used for current OwnID flow
        /// </summary>
        public FlowType FlowType { get; set; }

        /// <summary>
        ///     Indicate if current flow is Stateless flow
        /// </summary>
        public bool IsStateless =>
            FlowType == FlowType.Fido2Login
            || FlowType == FlowType.Fido2Register
            || FlowType == FlowType.Fido2Link
            || FlowType == FlowType.Fido2LinkWithPin
            || FlowType == FlowType.Fido2Recover
            || FlowType == FlowType.Fido2RecoverWithPin;

        /// <summary>
        ///     Get CacheItem auth type string representation
        /// </summary>
        /// <returns>auth type string representation</returns>
        public string GetAuthType()
        {
            if (FlowType == FlowType.Fido2Login
                || FlowType == FlowType.Fido2Register
                || FlowType == FlowType.Fido2Link
                || FlowType == FlowType.Fido2LinkWithPin
                || FlowType == FlowType.Fido2Recover
                || FlowType == FlowType.Fido2RecoverWithPin)
                return "FIDO2";

            return "Basic";
        }

        /// <summary>
        ///     Request Token from Web App
        /// </summary>
        public string RequestToken { get; set; }

        /// <summary>
        ///     Request Token to send to Web App
        /// </summary>
        public string ResponseToken { get; set; }

        /// <summary>
        ///     Payload
        /// </summary>
        /// <remarks>
        ///     General purpose payload to be used by integrated providers for theirs needs
        ///     (for example, account recover payload can store password reset token)
        /// </remarks>
        public string Payload { get; set; }

        /// <summary>
        ///     Prevents multiple concurrent assignments to current instance of <see cref="CacheItem" />
        /// </summary>
        public string ConcurrentId { get; set; }

        /// <summary>
        ///     Used for security checks (PIN and etc.)
        /// </summary>
        public string SecurityCode { get; set; }

        /// <summary>
        ///     Stores public key for partial auth flow
        /// </summary>
        public string PublicKey { get; set; }

        /// <summary>
        ///     Indicates if cache item can be used in middle of register / login process
        /// </summary>
        public bool IsValidForAuthorize => !HasFinalState &&
                                           (ChallengeType == ChallengeType.Register ||
                                            ChallengeType == ChallengeType.Login);

        /// <summary>
        ///     Indicates if cache item can be used in middle of link process
        /// </summary>
        public bool IsValidForLink => !HasFinalState && ChallengeType == ChallengeType.Link;

        /// <summary>
        ///     Indicates if cache item can be used in middle of recover process
        /// </summary>
        public bool IsValidForRecover => !HasFinalState && ChallengeType == ChallengeType.Recover;

        /// <summary>
        ///     Indicated if cache item has final state and status can not be changed
        /// </summary>
        public bool HasFinalState => Status == CacheItemStatus.Finished || Status == CacheItemStatus.Declined;

        /// <summary>
        ///     Fido2 counter
        /// </summary>
        public uint? Fido2SignatureCounter { get; set; }

        /// <summary>
        ///     Fido2 credential id
        /// </summary>
        public string Fido2CredentialId { get; set; }

        /// <summary>
        ///     Error
        /// </summary>
        public string Error { get; set; }

        /// <summary>
        ///     Passwordless recovery token part
        /// </summary>
        public string PasswordlessRecoveryToken { get; set; }

        /// <summary>
        ///     WebApp recovery token part
        /// </summary>
        public string WebAppRecoveryToken { get; set; }

        /// <summary>
        ///     Connection recovery token
        /// </summary>
        public string RecoveryToken => !string.IsNullOrEmpty(WebAppRecoveryToken)
            ? $"{PasswordlessRecoveryToken}:::{WebAppRecoveryToken}"
            : string.Empty;

        /// <summary>
        ///     Connection recovery data
        /// </summary>
        public string RecoveryData { get; set; }

        /// <summary>
        ///     Connection recovery token
        /// </summary>
        public string EncToken => $"{PasswordlessEncToken}:::{WebAppEncToken}";

        /// <summary>
        ///     Private key encryption passphrase
        /// </summary>
        public string PasswordlessEncToken { get; set; }

        /// <summary>
        ///     Private key encryption passphrase
        /// </summary>
        public string WebAppEncToken { get; set; }

        /// <summary>
        ///     Creates new instance of <see cref="CacheItem" /> based on <see cref="Nonce" /> and <see cref="DID" />
        /// </summary>
        public object Clone()
        {
            return new CacheItem
            {
                DID = DID,
                Nonce = Nonce,
                ChallengeType = ChallengeType,
                FlowType = FlowType,
                Status = Status,
                RequestToken = RequestToken,
                ResponseToken = ResponseToken,
                Context = Context,
                Payload = Payload,
                ConcurrentId = ConcurrentId,
                SecurityCode = SecurityCode,
                PublicKey = PublicKey,
                Fido2SignatureCounter = Fido2SignatureCounter,
                Fido2CredentialId = Fido2CredentialId,
                Error = Error,
                RecoveryData = RecoveryData,
                PasswordlessRecoveryToken = PasswordlessRecoveryToken,
                WebAppRecoveryToken = WebAppRecoveryToken,
                PasswordlessEncToken = PasswordlessEncToken,
                WebAppEncToken = WebAppEncToken
            };
        }
    }
}