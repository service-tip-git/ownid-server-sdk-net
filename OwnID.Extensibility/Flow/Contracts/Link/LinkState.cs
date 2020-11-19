namespace OwnID.Extensibility.Flow.Contracts.Link
{
    public class LinkState
    {
        public LinkState(string did, uint connectedDevicesCount)
        {
            DID = did;
            ConnectedDevicesCount = connectedDevicesCount;
        }

        /// <summary>
        ///     User unique identifier
        /// </summary>
        public string DID { get; }

        /// <summary>
        ///     Current connected devices count
        /// </summary>
        public uint ConnectedDevicesCount { get; }
    }
}