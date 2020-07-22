using System;
using System.Security.Cryptography;
using System.Text;

namespace OwnIdSdk.NetCore3.Web.Gigya
{
    public static class HashExtensions
    {
        public static string GetSha256(this string str)
        {
            using var sha256 = new SHA256Managed();

            var b64 = Encoding.UTF8.GetBytes(str);
            var hash = sha256.ComputeHash(b64);

            return Convert.ToBase64String(hash);
        }
    }
}