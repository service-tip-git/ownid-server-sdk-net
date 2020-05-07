using System;

namespace OwnIdSdk.NetCore3.Extensions
{
    internal static class GuidExtensions
    {
        internal static string ToShortString(this Guid guid)
        {
            var base64Guid = Convert.ToBase64String(guid.ToByteArray());
            base64Guid = base64Guid.Replace('+', '-').Replace('/', '_');
            return base64Guid.Substring(0, base64Guid.Length - 2);
        }

        internal static Guid FromShortStringToGuid(this string str)
        {
            str = str.Replace('_', '/').Replace('-', '+');
            var byteArray = Convert.FromBase64String(str + "==");
            return new Guid(byteArray);
        }
    }
}