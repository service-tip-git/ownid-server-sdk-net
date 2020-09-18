using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using OwnIdSdk.NetCore3.Extensibility.Services;

namespace OwnIdSdk.NetCore3.Extensibility.Configuration.Profile
{
    /// <summary>
    ///     Provides validation rules specific data for further localization
    /// </summary>
    public class ProfileValidationRuleMetadata
    {
        private readonly ValidationAttribute _validationAttribute;

        /// <summary>
        ///     Validator custom attributes specific for different type of validations
        /// </summary>
        public readonly Dictionary<string, string> Parameters = new Dictionary<string, string>();

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="validationAttribute">validation attribute</param>
        public ProfileValidationRuleMetadata(ValidationAttribute validationAttribute)
        {
            _validationAttribute = validationAttribute;
        }


        /// <summary>
        ///     OwnId application validator type
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        ///     Marks if provided by <see cref="FormatErrorMessage" /> value needs to be localized with
        ///     <see cref="ILocalizationService" />
        /// </summary>
        /// <remarks>
        ///     Should be <c>false</c> if <see cref="FormatErrorMessage" /> value is already localized by user with validation
        ///     attributes <c>ErrorResourceType</c> and <c>ErrorResourceName</c>
        /// </remarks>
        public bool NeedsInternalLocalization { get; set; }

        /// <summary>
        ///     Error key
        /// </summary>
        /// <remarks>Being used to localize validation errors</remarks>
        public string ErrorKey { get; set; }


        /// <summary>
        ///     Formats the localized error message to present to the user.
        /// </summary>
        /// <param name="name">The user-visible name to include in the formatted message.</param>
        /// <param name="errorMessage">localized error message</param>
        /// <returns>The localized string describing the validation error</returns>
        public string FormatErrorMessage(string name, string errorMessage = null)
        {
            if (!string.IsNullOrEmpty(errorMessage))
                _validationAttribute.ErrorMessage = errorMessage;

            return _validationAttribute.FormatErrorMessage(name);
        }
    }
}