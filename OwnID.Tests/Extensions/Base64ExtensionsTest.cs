using FluentAssertions;
using OwnID.Extensions;
using Xunit;

namespace OwnID.Tests.Extensions
{
    public class Base64ExtensionsTest
    {
        private const string Base64Symbols = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/";
        private const string Base64EncodedSymbols = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789-_";

        [Fact]
        public void EncodeBase64String_Test()
        {
            Base64Symbols.EncodeBase64String().Should().Be(Base64EncodedSymbols);
        }

        [Fact]
        public void DecodeBase64String_Test()
        {
            Base64EncodedSymbols.DecodeBase64String().Should().Be(Base64Symbols);
        }
    }
}