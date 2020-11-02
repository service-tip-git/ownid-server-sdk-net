using System;
using System.Linq;

namespace OwnIdSdk.NetCore3.Extensions
{
    public static class UrlExtensions
    {
        public static string GetWebAppBaseDomain(this Uri uri)
        {
            var segments = uri.DnsSafeHost.Split('.');

            return segments.Length == 1 ? uri.DnsSafeHost : "ownid.com";
        }
    }
}