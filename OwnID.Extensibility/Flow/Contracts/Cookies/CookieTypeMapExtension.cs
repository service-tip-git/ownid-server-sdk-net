using System;

namespace OwnID.Extensibility.Flow.Contracts.Cookies
{
    public static class CookieTypeMapExtension
    {
        public static string ToConstantString(this CookieType cookieType)
        {
            return cookieType switch
            {
                CookieType.Fido2 => CookieValuesConstants.Fido2CookieType,
                CookieType.Basic => CookieValuesConstants.BasicCookieType,
                CookieType.Passcode => CookieValuesConstants.PasscodeCookieType,
                CookieType.Recovery => CookieValuesConstants.RecoveryCookieType,
                _ => throw new ArgumentOutOfRangeException(nameof(cookieType), cookieType, null)
            };
        }

        public static CookieType ToCookieType(this string cookieTypeStr)
        {
            return cookieTypeStr switch
            {
                CookieValuesConstants.Fido2CookieType => CookieType.Fido2,
                CookieValuesConstants.BasicCookieType => CookieType.Basic,
                CookieValuesConstants.PasscodeCookieType => CookieType.Passcode,
                CookieValuesConstants.RecoveryCookieType => CookieType.Recovery,
                _ => throw new ArgumentOutOfRangeException(nameof(cookieTypeStr), typeof(CookieType), null)
            };
        }
    }
}