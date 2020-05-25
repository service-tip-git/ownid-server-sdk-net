namespace OwnIdSdk.NetCore3
{
    public interface ILocalizationService
    {
        string GetLocalizedString(string key, bool defaultAsAlternative = false);
    }
}