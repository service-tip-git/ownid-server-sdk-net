namespace OwnIdSdk.NetCore3.Extensions
{
    public static class Base64Extensions
    {
        public static string GetUrlEncodeString(this string input)
        {
            return input.Replace('+', '-').Replace('/', '_');
        }

        public static string GetUrlDecodeString(this string input)
        {
            return input.Replace('-', '+').Replace('_', '/');
        }
    }
}