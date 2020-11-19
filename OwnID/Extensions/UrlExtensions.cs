using System;

namespace OwnID.Extensions
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