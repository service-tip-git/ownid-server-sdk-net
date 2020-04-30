using System;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace OwnIdSdk.NetCore3.Cryptography
{
    public static class RsaHelper
    {
        private const string TopWrapper = "-----BEGIN PUBLIC KEY-----";
        private const string BottomWrapper = "\n-----END PUBLIC KEY-----";
        private const string BeginString = "-----BEGIN ";
        private const string EndString = "-----END ";

        /// <summary>
        ///     Instantiate RSA object with public and private key
        /// </summary>
        /// <param name="publicKey">Public key in base64 format</param>
        /// <param name="privateKey">Private key in base64 format</param>
        public static RSA LoadKeys(string publicKey, string privateKey = null)
        {
            if (publicKey.Length % 4 != 0 || privateKey != null && privateKey.Length % 4 != 0)
                throw new IOException("base64 data appears to be truncated");

            var rsa = RSA.Create();
            rsa.ImportSubjectPublicKeyInfo(new ReadOnlySpan<byte>(Convert.FromBase64String(publicKey)), out _);

            if (privateKey != null)
                rsa.ImportRSAPrivateKey(new ReadOnlySpan<byte>(Convert.FromBase64String(privateKey)), out _);

            return rsa;
        }

        /// <summary>
        ///     Read key from PEM encoded file
        /// </summary>
        /// <param name="reader">PEM file reader</param>
        /// <returns>Base64 key</returns>
        public static string ReadKeyFromPem(TextReader reader)
        {
            var line = reader.ReadLine();

            if (line == null || !StartsWith(line, BeginString))
                return null;

            line = line.Substring(BeginString.Length);
            var index = line.IndexOf('-');

            if (index <= 0 || !EndsWith(line, "-----") || line.Length - index != 5)
                return null;

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
                throw new IOException(endMarker + " not found");

            if (buf.Length % 4 != 0)
                throw new IOException("base64 data appears to be truncated");

            return buf.ToString();
        }

        public static string GetPublicKeyForTransfer(RSA rsa)
        {
            return $"{TopWrapper}\n{Convert.ToBase64String(rsa.ExportRSAPublicKey())}\n{BottomWrapper}";
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