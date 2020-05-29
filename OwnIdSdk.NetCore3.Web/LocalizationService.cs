using Microsoft.Extensions.Localization;

namespace OwnIdSdk.NetCore3.Web
{
    /// <summary>
    ///     Default localization mechanism
    /// </summary>
    /// <remarks>
    ///     Uses default and userDefined localizer
    /// </remarks>
    /// <inheritdoc cref="ILocalizationService" />
    public class LocalizationService : ILocalizationService
    {
        private readonly IStringLocalizer _customLocalizer;
        private readonly bool _customWasSet;
        private readonly IStringLocalizer _defaultLocalizer;
        private readonly bool _disabled = false;

        /// <param name="defaultLocalizer">Default localizer provided by OwnId SDK</param>
        /// <param name="userDefinedLocalizer">User defined localizer</param>
        public LocalizationService(IStringLocalizer defaultLocalizer, IStringLocalizer userDefinedLocalizer = null)
        {
            _defaultLocalizer = defaultLocalizer;

            if (userDefinedLocalizer != null)
            {
                _customLocalizer = userDefinedLocalizer;
                _customWasSet = true;
            }
            else
            {
                _customLocalizer = defaultLocalizer;
            }
        }

        public string GetLocalizedString(string key, bool defaultAsAlternative = false)
        {
            if (_disabled)
                return key;

            var originalItem = _customLocalizer[key];

            if (originalItem.ResourceNotFound && defaultAsAlternative && _customWasSet)
            {
                var defaultItem = _defaultLocalizer[key];

                if (!defaultItem.ResourceNotFound)
                    return defaultItem.Value;
            }

            return originalItem.Value;
        }
    }
}