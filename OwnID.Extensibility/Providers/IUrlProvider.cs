using System;
using OwnID.Extensibility.Flow;

namespace OwnID.Extensibility.Providers
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
        /// Generates url to accept start Own ID process
        /// </summary>
        /// <param name="context">Process unique identifier</param>
        /// <returns>Well-formatted url to accept start Own ID process</returns>
        Uri GetAcceptStartFlowUrl(string context);

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
        ///     Generates url with redirection to Web App with nested callback url (to server side endpoint)
        /// </summary>
        /// <param name="subUrl">Nested url that will be used as callback</param>
        /// <param name="language">language</param>
        /// <param name="requestToken">Request token</param>
        /// <param name="responseToken">Response token</param>
        /// <returns>Well-formatted url for Web App with callback option</returns>
        Uri GetWebAppSignWithCallbackUrl(Uri subUrl, string language, string requestToken = null, string responseToken = null);

        /// <summary>
        ///     Generates url to Web App connections
        /// </summary>
        /// <returns>Well-formatted url for Web App connections</returns>
        Uri GetWebAppConnectionsUrl();

        /// <summary>
        ///     Generates url with redirection to Fido2 path with nested callback url (to Web App)
        /// </summary>
        /// <param name="context">Process unique identifier</param>
        /// <param name="requestToken">Request token</param>
        /// <param name="language">language</param>
        /// <returns>Well-formatted url for Fido2 page with callback option</returns>
        Uri GetFido2Url(string context, string requestToken, string language);

        /// <summary>
        ///     Generates internal connection restore url
        /// </summary>
        /// <param name="context">Process unique identifier</param>
        /// <returns>Well-formatted url for internal connection restore</returns>
        Uri GetConnectionRecoveryUrl(string context);

        /// <summary>
        ///     Generates internal connection restore url
        /// </summary>
        /// <param name="context">Process unique identifier</param>
        /// <returns>Well-formatted url to check user existence</returns>
        Uri GetUserExistenceUrl(string context);

        /// <summary>
        ///     Generates reset passcode url
        /// </summary>
        /// <param name="context">Process unique identifier</param>
        /// <returns>Well-formatted url to reset passcode</returns>
        Uri GetResetPasscodeUrl(string context);
    }
}