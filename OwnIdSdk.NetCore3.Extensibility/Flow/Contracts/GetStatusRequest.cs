namespace OwnIdSdk.NetCore3.Extensibility.Flow.Contracts
{
    /// <summary>
    ///     POST /ownid/status request item structure
    /// </summary>
    public class GetStatusRequest
    {
        /// <summary>
        ///     Context
        /// </summary>
        public string Context { get; set; }

        /// <summary>
        ///     Nonce
        /// </summary>
        public string Nonce { get; set; }
    }
}