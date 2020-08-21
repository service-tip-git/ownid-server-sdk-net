using System;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace OwnIdSdk.NetCore3.Cryptography
{
    /// <summary>
    ///     Extends <see cref="RSA" /> mechanism for basic operations used in OwnIdSdk
    /// </summary>
    public static class RsaHelper
    {
        private const string PublicPkcs1Type = "RSA PUBLIC KEY";
        private const string PrivatePkcs1Type = "RSA PRIVATE KEY";
        private const string PrivatePkcs8Type = "PRIVATE KEY";
        private const string PublicSpkiType = "PUBLIC KEY";
        private const string HeaderSeparator = "-----";
        private const string BeginString = HeaderSeparator + "BEGIN ";
        private const string EndString = HeaderSeparator + "END ";

        public static RSA LoadKeys(TextReader publicKeyReader, TextReader privateKey = null)
        {
            var rsa = RSA.Create();
            var publicKeyData = ReadKey(publicKeyReader);

            switch (publicKeyData.definitionWrapper)
            {
                case PublicPkcs1Type:
                    rsa.ImportRSAPublicKey(new ReadOnlySpan<byte>(Convert.FromBase64String(publicKeyData.key)), out _);
                    break;
                case PublicSpkiType:
                    rsa.ImportSubjectPublicKeyInfo(new ReadOnlySpan<byte>(Convert.FromBase64String(publicKeyData.key)),
                        out _);
                    break;
                default:
                    throw new NotSupportedException($"Not supported header {publicKeyData.definitionWrapper}");
            }

            if (privateKey != null)
            {
                var privateKeyData = ReadKey(privateKey);

                switch (privateKeyData.definitionWrapper)
                {
                    case PrivatePkcs1Type:
                        rsa.ImportRSAPrivateKey(new ReadOnlySpan<byte>(Convert.FromBase64String(privateKeyData.key)),
                            out _);
                        break;
                    case PrivatePkcs8Type:
                        rsa.ImportPkcs8PrivateKey(new ReadOnlySpan<byte>(Convert.FromBase64String(privateKeyData.key)),
                            out _);
                        break;
                    default:
                        throw new NotSupportedException($"Not supported header {privateKeyData.definitionWrapper}");
                }
            }

            return rsa;
        }

        public static string ExportPublicKeyToPkcsFormattedString(RSA rsa)
        {
            return
                $"{BeginString}{PublicSpkiType}{HeaderSeparator}{Environment.NewLine}{Convert.ToBase64String(rsa.ExportSubjectPublicKeyInfo())}{Environment.NewLine}{EndString}{PublicSpkiType}{HeaderSeparator}";
        }

        private static (string definitionWrapper, string key) ReadKey(TextReader reader)
        {
            var line = reader.ReadLine();

            if (line == null || !StartsWith(line, BeginString))
                throw new FormatException($"No defining wrappers found '{BeginString}'");

            line = line.Substring(BeginString.Length);
            var index = line.IndexOf('-');

            if (index <= 0 || !EndsWith(line, HeaderSeparator) || line.Length - index != 5)
                throw new FormatException($"No defining wrappers found '{EndString}'");

            var type = line.Substring(0, index);

            var endMarker = EndString + type;
            var buf = new StringBuilder();

            while ((line = reader.ReadLine()) != null
                   && IndexOf(line, endMarker) == -1)
            {
                var colonPos = line.IndexOf(':');

                if (colonPos != -1)
                    // header fields are not supported
                    continue;

                buf.Append(line.Trim());
            }

            if (line == null)
                throw new FormatException($"End wrapper {endMarker} not found");

            if (buf.Length % 4 != 0)
                throw new FormatException("Base64 data appears to be truncated");

            return (type, buf.ToString());
        }

        private static bool StartsWith(string source, string prefix)
        {
            return CultureInfo.InvariantCulture.CompareInfo.IsPrefix(source, prefix, CompareOptions.Ordinal);
        }

        private static bool EndsWith(string source, string suffix)
        {
            return CultureInfo.InvariantCulture.CompareInfo.IsSuffix(source, suffix, CompareOptions.Ordinal);
        }

        private static int IndexOf(string source, string value)
        {
            return CultureInfo.InvariantCulture.CompareInfo.IndexOf(source, value, CompareOptions.Ordinal);
        }
    }
}