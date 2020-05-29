using System;

namespace OwnIdSdk.NetCore3.Store
{
    /// <summary>
    /// Auth flow detection fields store streucture
    /// </summary>
    public class CacheItem : ICloneable
    {
        /// <summary>
        /// Nonce
        /// </summary>
        public string Nonce { get; set; }
        
        /// <summary>
        /// Auth flow unique identifier
        /// </summary>
        public string DID { get; set; }
        
        /// <summary>
        /// Creates new instance of <see cref="CacheItem"/> based on <see cref="Nonce"/> and <see cref="DID"/>
        /// </summary>
        public object Clone()
        {
            return new CacheItem
            {
                DID = DID,
                Nonce = Nonce
            };
        }
    }
}