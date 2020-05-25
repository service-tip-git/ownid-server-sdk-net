using System.Reflection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OwnIdSdk.NetCore3.Configuration;
using OwnIdSdk.NetCore3.Web.Resources;

namespace OwnIdSdk.NetCore3.Web
{
    public class LocalizationService : ILocalizationService
    {
        private readonly IStringLocalizer _localizer;
        private readonly IStringLocalizer _defaultLocalizer;
        private readonly bool _customWasSet;
        private readonly bool _disabled;

        public LocalizationService(IStringLocalizerFactory factory, IOptions<OwnIdConfiguration> configuration)
        {
            if (configuration.Value.IgnoreInternalLocalization)
            {
                _disabled = configuration.Value.IgnoreInternalLocalization;
                return;
            }
            
            var type = typeof(OwnIdSdkDefault);
            var assemblyName = new AssemblyName(type.GetTypeInfo().Assembly.FullName!);
            _defaultLocalizer = factory.Create(type.Name, assemblyName.Name);
            
            if (configuration.Value.LocalizationResourceType != null)
            {
                _localizer = factory.Create(configuration.Value.LocalizationResourceType);
                var customLocalizerAssemblyName =
                    new AssemblyName(configuration.Value.LocalizationResourceType.GetTypeInfo().Assembly.FullName!);
                var a = configuration.Value.LocalizationResourceType.Name.Replace("_", ".");
                _localizer = factory.Create(configuration.Value.LocalizationResourceName,
                    customLocalizerAssemblyName.Name);
                _customWasSet = true;
            }
            else
                _localizer = _defaultLocalizer;
        }

        public string GetLocalizedString(string key, bool defaultAsAlternative = false)
        {
            if (_disabled)
                return key;

            var originalItem = _localizer[key];

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