using System;

namespace OwnIdSdk.NetCore3.Store
{
    public class CacheItem : ICloneable
    {
        public string Nonce { get; set; }
        
        public string DID { get; set; }
        
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