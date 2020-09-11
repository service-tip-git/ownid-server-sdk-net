using System;
using OwnIdSdk.NetCore3.Extensibility.Flow;

namespace OwnIdSdk.NetCore3.Extensibility.Providers
{
    public interface IUrlProvider
    {
        /// <summary>
        ///     Generates url to start Own ID process
        /// </summary>
        /// <param name="context">Process unique identifier</param>
        /// <returns>Well-formatted url to start Own ID process</returns>
        Uri GetStartFlowUrl(string context);

        /// <summary>
        ///     Generates url for challenge communications
        /// </summary>
        /// <param name="context">Process unique identifier</param>
        /// <param name="challengeType">Type of challenge</param>
        /// <param name="prefix">Is workaround parameter for partial implementation</param>
        /// <returns>Well-formatted url to use for specific challenge communication</returns>
        Uri GetChallengeUrl(string context, ChallengeType challengeType, string prefix = null);

        /// <summary>
        ///     Generates url for checking Own ID process approval status
        /// </summary>
        /// <param name="context">Process unique identifier</param>
        /// <returns>Well-formatted url to check status of approval Own ID process</returns>
        Uri GetSecurityApprovalStatusUrl(string context);

        /// <summary>
        ///     Generated url with redirection to Web App with nested callback url (to server side endpoint)
        /// </summary>
        /// <param name="subUrl">Nested url that will be used as callback</param>
        /// <returns>Well-formatted url for Web App with callback option</returns>
        Uri GetWebAppSignWithCallbackUrl(Uri subUrl);

        /// <summary>
        /// Generates url to Web App connections
        /// </summary>
        /// <returns>Well-formatted url for Web App connections</returns>
        Uri GetWebAppConnectionsUrl();

        /// <summary>
        ///     Generate url with redirection to Fido2 path with nested callback url (to Web App)
        /// </summary>
        /// <param name="subUrl">Nested url</param>
        /// <param name="requestType">request type (only <see cref="ChallengeType.Register"/> and <see cref="ChallengeType.Login"/> are supported</param>
        /// <returns>Well-formatted url for Fido2 page with callback option</returns>
        Uri GetFido2Url(Uri subUrl, ChallengeType requestType);
    }
}