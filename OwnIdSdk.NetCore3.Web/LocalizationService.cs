using Microsoft.Extensions.Localization;

namespace OwnIdSdk.NetCore3.Web
{
    public class LocalizationService : ILocalizationService
    {
        private readonly IStringLocalizer _customLocalizer;
        private readonly bool _customWasSet;
        private readonly IStringLocalizer _defaultLocalizer;
        private readonly bool _disabled = false;

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