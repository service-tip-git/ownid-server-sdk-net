using OwnID.Extensibility.Cache;

namespace OwnID.Extensions
{
    public static class CacheItemOperations
    {
        public static void FinishFlow(this CacheItem item, string did, string publicKey)
        {
            item.DID = did;
            item.PublicKey = publicKey;
            item.Status = CacheItemStatus.Finished;
        }
    }
}