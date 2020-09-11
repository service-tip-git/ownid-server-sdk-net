using System;
using System.Web;
using OwnIdSdk.NetCore3.Extensibility.Configuration;
using OwnIdSdk.NetCore3.Extensibility.Flow;
using OwnIdSdk.NetCore3.Extensibility.Providers;

namespace OwnIdSdk.NetCore3.Providers
{
    /// <summary>
    ///     Generates urls to maintain Own ID process
    /// </summary>
    /// <inheritdoc cref="IUrlProvider" />
    public class UrlProvider : IUrlProvider
    {
        private readonly IOwnIdCoreConfiguration _coreConfiguration;

        public UrlProvider(IOwnIdCoreConfiguration coreConfiguration)
        {
            _coreConfiguration = coreConfiguration;
        }

        public Uri GetStartFlowUrl(string context)
        {
            return GetBaseActionUrl(context, "start");
        }

        //prefix param is workaround for partial flow
        public Uri GetChallengeUrl(string context, ChallengeType challengeType, string prefix = null)
        {
            var action = challengeType switch
            {
                ChallengeType.Link => "link",
                ChallengeType.Recover => "recover",
                _ => $"challenge{prefix}"
            };

            return GetBaseActionUrl(context, action);
        }

        public Uri GetSecurityApprovalStatusUrl(string context)
        {
            return GetBaseActionUrl(context, "approval-status");
        }

        public Uri GetWebAppSignWithCallbackUrl(Uri subUrl)
        {
            var deepLink = new UriBuilder(new Uri(_coreConfiguration.OwnIdApplicationUrl, "sign"));
            var query = HttpUtility.ParseQueryString(deepLink.Query);
            query["q"] = $"{subUrl.Authority}{subUrl.PathAndQuery}";
            deepLink.Query = query.ToString() ?? string.Empty;
            return deepLink.Uri;
        }

        public Uri GetFido2Url(Uri subUrl, ChallengeType requestType)
        {
            var deepLink = new UriBuilder(_coreConfiguration.Fido2.PasswordlessPageUrl);
            var query = HttpUtility.ParseQueryString(deepLink.Query);
            switch (requestType)
            {
                case ChallengeType.Link:
                case ChallengeType.Recover:
                case ChallengeType.Register:
                    query["t"] = "r";
                    break;
                case ChallengeType.Login:
                    query["t"] = "l";
                    break;
            }

            query["q"] = $"{subUrl.Authority}{subUrl.PathAndQuery}";
            deepLink.Query = query.ToString() ?? string.Empty;
            return deepLink.Uri;
        }

        public Uri GetWebAppConnectionsUrl()
        {
            return new Uri(_coreConfiguration.OwnIdApplicationUrl, "account");
        }

        private Uri GetBaseActionUrl(string context, string action)
        {
            var path = "";

            if (!string.IsNullOrEmpty(_coreConfiguration.CallbackUrl.PathAndQuery))
                path = _coreConfiguration.CallbackUrl.PathAndQuery.EndsWith("/")
                    ? _coreConfiguration.CallbackUrl.PathAndQuery
                    : _coreConfiguration.CallbackUrl.PathAndQuery + "/";

            return new Uri(_coreConfiguration.CallbackUrl, $"{path}ownid/{context}/{action}");
        }
    }
}