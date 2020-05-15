using System;
using System.Linq;
using Microsoft.Extensions.Options;

namespace OwnIdSdk.NetCore3.Configuration
{
    public class OwnIdConfigurationValidator : IValidateOptions<OwnIdConfiguration>
    {
        public ValidateOptionsResult Validate(string name, OwnIdConfiguration options)
        {
            var callbackResult =
                ValidateUri(nameof(options.CallbackUrl), options.CallbackUrl, options.IsDevEnvironment);

            if (callbackResult != ValidateOptionsResult.Success)
                return callbackResult;

            var ownIdApplicationUriResult = ValidateUri(nameof(options.OwnIdApplicationUrl),
                options.OwnIdApplicationUrl, options.IsDevEnvironment);

            if (ownIdApplicationUriResult != ValidateOptionsResult.Success)
                return ownIdApplicationUriResult;

            if (options.JwtSignCredentials == default)
                return ValidateOptionsResult.Fail($"{nameof(options.JwtSignCredentials)} are required");

            if (string.IsNullOrWhiteSpace(options.Requester.DID) || string.IsNullOrWhiteSpace(options.Requester.Name))
                return ValidateOptionsResult.Fail(
                    $"{nameof(options.Requester.DID)} and {nameof(options.Requester.Name)} are required");

            return options.ProfileConfiguration.Validate();
        }

        private ValidateOptionsResult ValidateUri(string name, Uri value, bool isDevEnvironment)
        {
            if (value == default)
                return ValidateOptionsResult.Fail($"{name} is required");

            if (!value.IsWellFormedOriginalString())
                return ValidateOptionsResult.Fail($"{name} is not valid url");

            if (!isDevEnvironment && value.Scheme != "https")
                return ValidateOptionsResult.Fail($"{name}: https is required for production use");
            
            if (isDevEnvironment && value.Scheme != "https" && value.Scheme != "http")
                return ValidateOptionsResult.Fail($"{name}: https or http are supported only");

            if ((bool) value.Query?.Any())
                return ValidateOptionsResult.Fail($"{name} should not contain query params");

            return ValidateOptionsResult.Success;
        }
    }
}