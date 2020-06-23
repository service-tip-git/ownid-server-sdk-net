using System;
using OwnIdSdk.NetCore3.Contracts.Jwt;

namespace OwnIdSdk.NetCore3.Store
{
    /// <summary>
    /// Auth flow detection fields store structure
    /// </summary>
    public class CacheItem : ICloneable
    {
        /// <summary>
        /// Auth flow unique identifier
        /// </summary>
        public string Context { get; set; }
        
        /// <summary>
        /// Nonce
        /// </summary>
        public string Nonce { get; set; }
        
        /// <summary>
        /// User unique identity
        /// </summary>
        public string DID { get; set; }
        
        /// <summary>
        /// Challenge type related to the <c>Context</c> and <see cref="Nonce"/>
        /// </summary>
        public ChallengeType ChallengeType { get; set; }
        
        /// <summary>
        /// Flags if auth process is finished
        /// </summary>
        public bool IsFinished { get; set; }
        
        /// <summary>
        /// Request Token from Web App
        /// </summary>
        public string RequestToken { get; set; }
        
        /// <summary>
        /// Request Token to send to Web App
        /// </summary>
        public string ResponseToken { get; set; }
        
        /// <summary>
        /// Creates new instance of <see cref="CacheItem"/> based on <see cref="Nonce"/> and <see cref="DID"/>
        /// </summary>
        public object Clone()
        {
            return new CacheItem
            {
                DID = DID,
                Nonce = Nonce,
                ChallengeType = ChallengeType,
                IsFinished = IsFinished,
                RequestToken = RequestToken,
                ResponseToken = ResponseToken,
                Context = Context
            };
        }
    }
}