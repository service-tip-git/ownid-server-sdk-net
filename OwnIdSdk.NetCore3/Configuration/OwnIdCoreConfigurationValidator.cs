using System;
using System.Linq;
using Microsoft.Extensions.Options;
using OwnIdSdk.NetCore3.Extensibility.Configuration;

namespace OwnIdSdk.NetCore3.Configuration
{
    /// <summary>
    ///     Stores <see cref="OwnIdCoreConfiguration" /> validation logic
    /// </summary>
    /// <remarks>Implements <see cref="IValidateOptions{TOptions}" /> default mechanism</remarks>
    public class OwnIdCoreConfigurationValidator : IValidateOptions<OwnIdCoreConfiguration>
    {
        public ValidateOptionsResult Validate(string name, OwnIdCoreConfiguration options)
        {
            if (!IsUriValid(nameof(options.CallbackUrl), options.CallbackUrl, options.IsDevEnvironment,
                out var callBackUrlValidationError))
                return ValidateOptionsResult.Fail(callBackUrlValidationError);

            if (!IsUriValid(nameof(options.OwnIdApplicationUrl), options.OwnIdApplicationUrl, options.IsDevEnvironment,
                out var ownIdAppUrlValidationError))
                return ValidateOptionsResult.Fail(ownIdAppUrlValidationError);

            if (options.JwtSignCredentials == default)
                return ValidateOptionsResult.Fail($"{nameof(options.JwtSignCredentials)} are required");

            if (string.IsNullOrWhiteSpace(options.DID) || string.IsNullOrWhiteSpace(options.Name))
                return ValidateOptionsResult.Fail(
                    $"{nameof(options.DID)} and {nameof(options.Name)} are required");

            if (options.CacheExpirationTimeout == 0)
                return ValidateOptionsResult.Fail(
                    $"{nameof(options.CacheExpirationTimeout)} can not be equal to 0");

            if (options.JwtExpirationTimeout == 0)
                return ValidateOptionsResult.Fail(
                    $"{nameof(options.JwtExpirationTimeout)} can not be equal to 0");

            var fido2Enabled = options.AuthenticationMode.IsFido2Enabled();
            
            // Validate Fido2Url
            if (fido2Enabled 
                && !IsUriValid(nameof(options.Fido2.PasswordlessPageUrl), options.Fido2.PasswordlessPageUrl,
                    options.IsDevEnvironment,
                    out var fido2ValidationError))
            {
                return ValidateOptionsResult.Fail(fido2ValidationError);
            }

            // Validate Fido2Origin
            if (fido2Enabled
                && !IsUriValid(nameof(options.Fido2.Origin), options.Fido2.Origin, options.IsDevEnvironment,
                    out var fido2OriginValidationError))
            {
                return ValidateOptionsResult.Fail(fido2OriginValidationError);
            }
            
            if(string.IsNullOrWhiteSpace(options.TopDomain))
                return ValidateOptionsResult.Fail(
                    $"{nameof(options.TopDomain)} is required");

            return options.ProfileConfiguration.Validate();
        }

        private bool IsUriValid(string name, Uri value, bool isDevEnvironment, out string error)
        {
            error = null;

            if (value == default)
            {
                error = $"{name} is required";
                return false;
            }

            if (!value.IsWellFormedOriginalString())
            {
                error = $"{name} is not valid url";
                return false;
            }

            if (!isDevEnvironment && value.Scheme != "https")
            {
                error = $"{name}: https is required for production use";
                return false;
            }

            if (isDevEnvironment && value.Scheme != "https" && value.Scheme != "http")
            {
                error = $"{name}: https or http are supported only";
                return false;
            }

            if ((bool) value.Query?.Any())
            {
                error = $"{name} should not contain query params";
                return false;
            }

            return true;
        }
    }
}