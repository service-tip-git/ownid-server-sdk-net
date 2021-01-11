using System;
using System.Linq;
using Microsoft.Extensions.Options;
using OwnID.Extensibility.Configuration;

namespace OwnID.Configuration
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

            if (string.IsNullOrWhiteSpace(options.TopDomain))
                return ValidateOptionsResult.Fail($"{nameof(options.TopDomain)} is required");

            // Validate Fido2 configuration
            var fido2Validator = new Fido2ConfigurationValidator();
            var fido2ValidationResult = fido2Validator.Validate(options.Fido2, options.IsDevEnvironment);
            if (fido2ValidationResult.Failed)
                return fido2ValidationResult;

            if (!options.Fido2.IsEnabled && options.Fido2FallbackBehavior == Fido2FallbackBehavior.Block)
            {
                return ValidateOptionsResult.Fail(
                    $"FIDO2 is disabled, but '{nameof(options.Fido2FallbackBehavior)}' is set to '{nameof(Fido2FallbackBehavior.Block)}'");
            }

            return options.ProfileConfiguration.Validate();
        }

        public static bool IsUriValid(string name, Uri value, bool isDevEnvironment, out string error)
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