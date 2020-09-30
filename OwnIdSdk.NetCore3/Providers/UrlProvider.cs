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

        public Uri GetWebAppSignWithCallbackUrl(Uri subUrl, string language)
        {
            var deepLink = new UriBuilder(new Uri(_coreConfiguration.OwnIdApplicationUrl, "sign"));
            var query = HttpUtility.ParseQueryString(deepLink.Query);
            query[QueryStringParameters.CallBackUrl] = $"{subUrl.Authority}{subUrl.PathAndQuery}";
            if (!string.IsNullOrWhiteSpace(language))
                query[QueryStringParameters.Language] = language;
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
                    query[QueryStringParameters.RequestType] = "r";
                    break;
                case ChallengeType.Login:
                    query[QueryStringParameters.RequestType] = "l";
                    break;
            }

            query[QueryStringParameters.CallBackUrl] = $"{subUrl.Authority}{subUrl.PathAndQuery}";
            query[QueryStringParameters.StateSuffix] = "state";
            deepLink.Query = query.ToString() ?? string.Empty;
            return deepLink.Uri;
        }

        public Uri GetConnectionRecoveryUrl(string context)
        {
            return GetBaseActionUrl(context, "conn-recovery");
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

        private static class QueryStringParameters
        {
            public const string CallBackUrl = "q";
            public const string Language = "l";
            public const string RequestType = "t";
            public const string StateSuffix = "s";
        }
    }
}