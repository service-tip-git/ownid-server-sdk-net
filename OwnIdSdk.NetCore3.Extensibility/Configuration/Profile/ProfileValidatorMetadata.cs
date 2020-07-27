using System;
using OwnIdSdk.NetCore3.Extensibility.Services;

namespace OwnIdSdk.NetCore3.Extensibility.Configuration.Profile
{
    /// <summary>
    ///     Provides validation rules specific data for further localization
    /// </summary>
    public class ProfileValidationRuleMetadata
    {
        /// <summary>
        ///     OwnId application validator type
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        ///     Marks if provided by <see cref="GetErrorMessageKey" /> value needs to be localized with
        ///     <see cref="ILocalizationService" />
        /// </summary>
        /// <remarks>
        ///     Should be <c>false</c> if <see cref="GetErrorMessageKey" /> value is already localized by user with validation
        ///     attributes <c>ErrorResourceType</c> and <c>ErrorResourceName</c>
        /// </remarks>
        public bool NeedsInternalLocalization { get; set; }

        /// <summary>
        ///     Provides validation error message that will be localized with <see cref="ILocalizationService" /> if
        ///     <see cref="NeedsInternalLocalization" /> is true
        /// </summary>
        public Func<string> GetErrorMessageKey { get; set; }
    }
}