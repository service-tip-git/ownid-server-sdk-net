using System;
using System.Net.Http;

namespace OwnID.Flow.Steps
{
    /// <summary>
    ///     Defines settings for HTTP-based call action
    /// </summary>
    public class CallAction
    {
        public CallAction(Uri url) : this(url, HttpMethod.Post.ToString())
        {
        }

        public CallAction(Uri url, string method)
        {
            Url = url;
            Method = method.ToUpper();
        }

        /// <summary>
        ///     Url that should be requested
        /// </summary>
        public Uri Url { get; set; }

        /// <summary>
        ///     HTTP method that should be used to request <see cref="Url" />
        /// </summary>
        public string Method { get; set; }
    }
}