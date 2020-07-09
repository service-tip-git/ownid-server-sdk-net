using System;

namespace OwnIdSdk.NetCore3.Flow
{
    /// <summary>
    ///     Defines settings for HTTP-based polling call action
    /// </summary>
    /// <inheritdoc cref="CallAction" />
    public class PollingAction : CallAction
    {
        public PollingAction(Uri url, string method, int interval) : base(url, method)
        {
            Interval = interval;
        }

        public PollingAction(Uri url, int interval) : base(url)
        {
            Interval = interval;
        }

        /// <summary>
        ///     Interval in milliseconds
        /// </summary>
        public int Interval { get; set; }
    }
}