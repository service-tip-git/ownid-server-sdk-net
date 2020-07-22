using System;
using OwnIdSdk.NetCore3.Contracts.Jwt;

namespace OwnIdSdk.NetCore3
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
        Uri GetWebAppWithCallbackUrl(Uri subUrl);
    }
}