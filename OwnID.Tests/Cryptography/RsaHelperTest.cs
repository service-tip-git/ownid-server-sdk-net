using System;
using System.IO;
using FluentAssertions;
using OwnID.Cryptography;
using Xunit;

namespace OwnID.Tests.Cryptography
{
    public class RsaHelperTest
    {
        private const string PublicKeyStartString = "-----BEGIN PUBLIC KEY-----\n";
        private const string PublicKeyEndString = "\n-----END PUBLIC KEY-----";

        private const string PublicBase64EncodedKey =
            @"MIICIjANBgkqhkiG9w0BAQEFAAOCAg8AMIICCgKCAgEAwRf/z+C5QOxWeDklDm1rThqeN94sBC00Fau/NYOU1PdgEkjrvP9NXhoHvUm7iv3CoLibjGad8lSeJ7my7sgE7EwnlFf5WyU9d74AEvGqJGmkjJfXc+0DC80eRx6nO9luDoqVpsv+wBVs0EbChwjYsZt51WZRJj6OO/W7eup062FPTP0HMy+uthpppnYWxZ2bY8tZN5IVRSUv5l0f4oMTskbM8Af75h6ICfqWOhXuv3SFSOS5viwx+beUB+px+wVm2xJBRWnlCL2l5uzyiP2STPr6ZZiDcOByBcq61eeF/lkSm9io5O66dLFE8zEH92hPJqsC70ZszXRVifg706ALlnO+8BIuTPwsEe01mrUm0sSy/yzgPAJd6BZ4u0WRNIsEpP2fPZbLvfpSllqiBSOFVJu7pnrsnuRIi9n4u+BTv7V49NcjMDDYJ0nvtOdcg4X5TgqFBpYYZHl39dOz0J7AzBAfh/zDs0DtHGVloxXUMtXFbBs/f2k37V9te8krsh5pCljDDoPOB4Iwc01yDxwj0DK7+XDYID1QHvi/Fl4vsmti1VCackMDhWP3cb9OxDB2OW6aa+QvKP8Co8hDG6j/bz0n+mXPbU/jQA2s1pNzKBgN2jF09pwAld5foTg/3QIh417S+hsPGWrlldr1MneMkGYACfNcJ8JCm5RwK4izypkCAwEAAQ==";

        private const string PrivateKeyStartString = "-----BEGIN RSA PRIVATE KEY-----\n";
        private const string PrivateKeyEndString = "\n-----END RSA PRIVATE KEY-----";

        private const string PrivateBase64EncodedKey =
            @"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";

        [Fact]
        public void ExportPublicKeyToPkcsFormattedString_Success()
        {
            var keyString = PublicKeyStartString + PublicBase64EncodedKey + PublicKeyEndString;
            using var publicKey = new StringReader(keyString);
            var rsa = RsaHelper.LoadKeys(publicKey);
            RsaHelper.ExportPublicKeyToPkcsFormattedString(rsa).Should().Be(keyString);
        }

        [Fact]
        public void LoadKeys_CorruptedBase64_Exception()
        {
            FluentActions.Invoking(() =>
            {
                using var invalidPublicKey =
                    new StringReader(PublicKeyStartString +
                                     PublicBase64EncodedKey.Substring(1, PublicBase64EncodedKey.Length - 1) +
                                     PublicKeyEndString);
                RsaHelper.LoadKeys(invalidPublicKey);
            }).Should().Throw<FormatException>();

            FluentActions.Invoking(() =>
            {
                using var validPublicKey =
                    new StringReader(PublicKeyStartString + PublicBase64EncodedKey + PublicKeyEndString);
                using var invalidPrivateKey = new StringReader(PrivateKeyStartString +
                                                               PrivateBase64EncodedKey.Substring(1,
                                                                   PublicBase64EncodedKey.Length - 1) +
                                                               PrivateKeyEndString);
                RsaHelper.LoadKeys(validPublicKey, invalidPrivateKey);
            }).Should().Throw<FormatException>();
        }

        [Fact]
        public void LoadKeys_PublicKeyOnly_Success()
        {
            using var publicKey = new StringReader(PublicKeyStartString + PublicBase64EncodedKey + PublicKeyEndString);
            var rsa = RsaHelper.LoadKeys(publicKey);
            rsa.KeySize.Should().Be(4096);
            PublicBase64EncodedKey.Should().Be(Convert.ToBase64String(rsa.ExportSubjectPublicKeyInfo()));
        }

        [Fact]
        public void LoadKeys_PublicPrivateKeys_Success()
        {
            using var publicKey = new StringReader(PublicKeyStartString + PublicBase64EncodedKey + PublicKeyEndString);
            using var privateKey =
                new StringReader(PrivateKeyStartString + PrivateBase64EncodedKey + PrivateKeyEndString);
            var rsa = RsaHelper.LoadKeys(publicKey, privateKey);
            rsa.KeySize.Should().Be(4096);
            PublicBase64EncodedKey.Should().Be(Convert.ToBase64String(rsa.ExportSubjectPublicKeyInfo()));
            PrivateBase64EncodedKey.Should().Be(Convert.ToBase64String(rsa.ExportRSAPrivateKey()));
        }
    }
}