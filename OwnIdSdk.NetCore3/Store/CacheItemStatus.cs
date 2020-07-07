namespace OwnIdSdk.NetCore3.Store
{
    /// <summary>
    /// Specifies the <see cref="CacheItem"/> state
    /// </summary>
    public enum CacheItemStatus
    {
        /// <summary>
        /// Started
        /// </summary>
        Started = 1,
        /// <summary>
        /// Processing started
        /// </summary>
        Processing = 2,
        /// <summary>
        /// Processing finished
        /// </summary>
        Finished = 3
    }
}