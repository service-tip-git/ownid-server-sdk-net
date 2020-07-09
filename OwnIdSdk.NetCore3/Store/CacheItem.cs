using System;
using OwnIdSdk.NetCore3.Contracts.Jwt;
using OwnIdSdk.NetCore3.Flow;

namespace OwnIdSdk.NetCore3.Store
{
    /// <summary>
    ///     OwnID flow detection fields store structure
    /// </summary>
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
        ///     <see cref="OwnIdSdk.NetCore3.Flow.FlowType" /> that should be used for current OwnID flow
        /// </summary>
        public FlowType FlowType { get; set; }

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
        ///     Indicates if cache item can be used in middle of register / login process
        /// </summary>
        public bool IsValidForLoginRegister => !HasFinalState &&
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
                SecurityCode = SecurityCode
            };
        }
    }
}