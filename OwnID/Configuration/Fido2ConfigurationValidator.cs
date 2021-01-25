using Microsoft.Extensions.Options;
using OwnID.Extensibility.Configuration;

namespace OwnID.Configuration
{
    public class Fido2ConfigurationValidator
    {
        public ValidateOptionsResult Validate(IFido2Configuration configuration, bool isDevEnvironment)
        {
            // Validate Fido2Url
            if (configuration.PasswordlessPageUrl != null && !OwnIdCoreConfigurationValidator.IsUriValid(
                nameof(configuration.PasswordlessPageUrl),
                configuration.PasswordlessPageUrl, isDevEnvironment, out _))
                return ValidateOptionsResult.Skip;

            return ValidateOptionsResult.Success;
        }
    }
}