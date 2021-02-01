using System;
using System.Web;
using OwnID.Extensibility.Configuration;
using OwnID.Extensibility.Flow;
using OwnID.Extensibility.Flow.Contracts.Start;
using OwnID.Extensibility.Providers;

namespace OwnID.Providers
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

        public Uri GetAcceptStartFlowUrl(string context)
        {
            return GetBaseActionUrl(context, "start/accept");
        }

        // prefix param is workaround for partial flow
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

        public Uri GetWebAppSignWithCallbackUrl(Uri subUrl, string language, string requestToken = null,
            string responseToken = null)
        {
            var deepLink = new UriBuilder(new Uri(_coreConfiguration.OwnIdApplicationUrl, "sign"));
            var query = HttpUtility.ParseQueryString(deepLink.Query);
            query[QueryStringParameters.CallBackUrl] = $"{subUrl.Authority}{subUrl.PathAndQuery}";

            if (!string.IsNullOrEmpty(requestToken) && !string.IsNullOrEmpty(responseToken))
            {
                query[QueryStringParameters.RequestToken] = requestToken;
                query[QueryStringParameters.ResponseToken] = responseToken;
            }

            if (!string.IsNullOrWhiteSpace(language))
                query[QueryStringParameters.Language] = language;

            deepLink.Query = query.ToString() ?? string.Empty;
            return deepLink.Uri;
        }

        public Uri GetConnectionRecoveryUrl(string context)
        {
            return GetBaseActionUrl(context, "conn-recovery");
        }

        public Uri GetUserExistenceUrl(string context)
        {
            return GetBaseActionUrl(context, "users/existence");
        }

        public Uri GetResetPasscodeUrl(string context)
        {
            return GetBaseActionUrl(context, "passcode");
        }

        public Uri GetStopFlowUrl(string context)
        {
            return GetBaseActionUrl(context, "stop");
        }

        public Uri GetSwitchAuthTypeUrl(string context, ConnectionAuthType authType)
        {
            return GetBaseActionUrl(context, $"auth-type/{authType.ToString().ToLower()}");
        }

        public Uri GetWebAppConnectionsUrl()
        {
            return new(_coreConfiguration.OwnIdApplicationUrl, "account");
        }

        public Uri GetFido2Url(string context, string requestToken, string language)
        {
            var fido2Url = new UriBuilder(_coreConfiguration.Fido2.PasswordlessPageUrl);
            var query = HttpUtility.ParseQueryString(fido2Url.Query);

            // settings url
            var settingsUrl = new UriBuilder(GetBaseActionUrl(context, "fido2/settings"));
            var settingsQuery = HttpUtility.ParseQueryString(fido2Url.Query);
            settingsQuery[QueryStringParameters.RequestToken] = requestToken;
            
            if (!string.IsNullOrWhiteSpace(language))
            {
                query[QueryStringParameters.Language] = language;
                settingsQuery[QueryStringParameters.Language] = language;
            }

            settingsUrl.Query = settingsQuery.ToString() ?? string.Empty;
            query[QueryStringParameters.CallBackUrl] = settingsUrl.Uri.ToString();
            fido2Url.Query = query.ToString() ?? string.Empty;
            return fido2Url.Uri;
        }

        public Uri GetStartFlowUrl(string context)
        {
            return GetBaseActionUrl(context, "start");
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
            public const string RequestToken = "rt";
            public const string ResponseToken = "rst";
        }
    }
}