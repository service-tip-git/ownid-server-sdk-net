namespace OwnIdSdk.NetCore3.Extensibility.Cache
{
    /// <summary>
    ///     Specifies the <see cref="CacheItem" /> state
    /// </summary>
    public enum CacheItemStatus
    {
        /// <summary>
        ///     Item was created but no interaction was made
        /// </summary>
        Initiated = 1,

        /// <summary>
        ///     Processing started
        /// </summary>
        Started = 2,

        /// <summary>
        ///     Waiting for user to approve the action
        /// </summary>
        WaitingForApproval = 3,

        /// <summary>
        ///     Action approved by user
        /// </summary>
        Approved = 4,

        /// <summary>
        ///     Action declined by user
        /// </summary>
        Declined = 5,

        /// <summary>
        ///     Process is finished
        /// </summary>
        Finished = 99
    }
}