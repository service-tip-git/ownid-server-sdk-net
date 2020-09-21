namespace OwnIdSdk.NetCore3.Extensions
{
    public static class Base64Extensions
    {
        public static string EncodeBase64String(this string input)
        {
            return input.Replace('+', '-').Replace('/', '_');
        }

        public static string DecodeBase64String(this string input)
        {
            return input.Replace('-', '+').Replace('_', '/');
        }
    }
}