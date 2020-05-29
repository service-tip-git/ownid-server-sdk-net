namespace OwnIdSdk.NetCore3
{
    /// <summary>
    ///     Describes localization mechanism
    /// </summary>
    public interface ILocalizationService
    {
        /// <summary>
        ///     Get localized string by key in resources
        /// </summary>
        /// <param name="key">Resource key</param>
        /// <param name="defaultAsAlternative">
        ///     Check default localization flag. <c>true</c> needs to try localize with default
        ///     localization resource if eky was not found in custom localization resource
        /// </param>
        /// <returns>Localized string or key as is if no such key was found in localization resources</returns>
        string GetLocalizedString(string key, bool defaultAsAlternative = false);
    }
}