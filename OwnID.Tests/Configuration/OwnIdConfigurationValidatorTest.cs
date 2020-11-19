using System;
using System.Security.Cryptography;
using FluentAssertions;
using OwnID.Configuration;
using OwnID.Extensibility.Configuration;
using Xunit;

namespace OwnID.Tests.Configuration
{
    public class OwnIdConfigurationValidatorTest : IDisposable
    {
        private readonly RSA _sign;
        private readonly OwnIdCoreConfigurationValidator _validator;

        public OwnIdConfigurationValidatorTest()
        {
            _sign = RSA.Create();
            _validator = new OwnIdCoreConfigurationValidator();
        }

        public void Dispose()
        {
            _sign?.Dispose();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("http://test.com/")]
        [InlineData(
            "https://www.test.com/search/Le+Venezuela+b%C3%A9n%C3%A9ficie+d%27importantes+ressources+naturelles+%3A+p%C3%A9trole%2C+gaz%2C+mines")]
        [InlineData("https://test.com/?a=1")]
        public void Validate_Invalid_CallbackUrl(string value)
        {
            var config = GetValidConfiguration();
            config.OwnIdApplicationUrl = value != null ? new Uri(value) : null;
            _validator.Validate(string.Empty, config).Failed.Should().BeTrue();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("http://test.com/")]
        [InlineData(
            "https://www.test.com/search/Le+Venezuela+b%C3%A9n%C3%A9ficie+d%27importantes+ressources+naturelles+%3A+p%C3%A9trole%2C+gaz%2C+mines")]
        [InlineData("https://test.com/?a=1")]
        public void Validate_Invalid_OwnIdApplicationUrl(string value)
        {
            var config = GetValidConfiguration();
            config.OwnIdApplicationUrl = value != null ? new Uri(value) : null;
            _validator.Validate(string.Empty, config).Failed.Should().BeTrue();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("  ")]
        public void Validate_Invalid_Requester_DID(string value)
        {
            var config = GetValidConfiguration();
            config.DID = value;
            _validator.Validate(string.Empty, config).Failed.Should().BeTrue();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("  ")]
        public void Validate_Invalid_Requester_Name(string value)
        {
            var config = GetValidConfiguration();
            config.Name = value;
            _validator.Validate(string.Empty, config).Failed.Should().BeTrue();
        }

        private OwnIdCoreConfiguration GetValidConfiguration()
        {
            var config = new OwnIdCoreConfiguration
            {
                OwnIdApplicationUrl = new Uri("https://ownid.com:12/sign"),
                CallbackUrl = new Uri("https://localhost:12"),
                DID = "did",
                Name = "name",
                JwtSignCredentials = _sign,
                CacheExpirationTimeout = 600000,
                JwtExpirationTimeout = 60000,
                TopDomain = "ownid.com"
            };

            config.Fido2.Origin = new Uri("https://test.com");
            config.Fido2.PasswordlessPageUrl = new Uri("https://test.com");

            return config;
        }

        [Fact]
        public void Validate_Invalid_JwtSignCredentials()
        {
            var config = GetValidConfiguration();
            config.JwtSignCredentials = null;
            _validator.Validate(string.Empty, config).Failed.Should().BeTrue();
        }

        [Theory]
        [InlineData(true, null, "{0} is required")]
        [InlineData(true, @"c:\dir\file", "{0} is not valid url")]
        [InlineData(false, "http://ownid.com", "{0}: https is required for production use")]
        [InlineData(true, "ftp://ownid.com", "{0}: https or http are supported only")]
        [InlineData(true, "http://ownid.com?param=val", "{0} should not contain query params")]
        public void Validate_Invalid_Fido2Url(bool isDev, string fido2Url, string errorMessage)
        {
            var config = GetValidConfiguration();
            config.AuthenticationMode = AuthenticationModeType.All;

            config.IsDevEnvironment = isDev;
            config.Fido2.PasswordlessPageUrl = !string.IsNullOrEmpty(fido2Url) ? new Uri(fido2Url) : null;

            var result = _validator.Validate(string.Empty, config);
            var expectedError = string.Format(errorMessage, nameof(config.Fido2.PasswordlessPageUrl));

            result.Failed.Should().BeTrue();
            result.FailureMessage.Should().BeEquivalentTo(expectedError);
        }

        [Theory]
        [InlineData(true, @"http://ownid.com")]
        [InlineData(false, "https://ownid.com")]
        public void Validate_Valid_Fido2Url(bool isDev, string fido2Url)
        {
            var config = GetValidConfiguration();
            config.AuthenticationMode = AuthenticationModeType.All;

            config.Fido2.PasswordlessPageUrl = new Uri(fido2Url);
            config.IsDevEnvironment = isDev;

            var result = _validator.Validate(string.Empty, config);

            result.Succeeded.Should().BeTrue();
        }

        [Theory]
        [InlineData(true, null, "{0} is required")]
        [InlineData(true, @"c:\dir\file", "{0} is not valid url")]
        [InlineData(false, "http://ownid.com", "{0}: https is required for production use")]
        [InlineData(true, "ftp://ownid.com", "{0}: https or http are supported only")]
        [InlineData(true, "http://ownid.com?param=val", "{0} should not contain query params")]
        public void Validate_Invalid_Origin(bool isDev, string originUrl, string errorMessage)
        {
            var config = GetValidConfiguration();
            config.AuthenticationMode = AuthenticationModeType.All;

            config.IsDevEnvironment = isDev;
            config.Fido2.Origin = !string.IsNullOrEmpty(originUrl) ? new Uri(originUrl) : null;

            var result = _validator.Validate(string.Empty, config);
            var expectedError = string.Format(errorMessage, nameof(config.Fido2.Origin));

            result.Failed.Should().BeTrue();
            result.FailureMessage.Should().BeEquivalentTo(expectedError);
        }

        [Theory]
        [InlineData(true, @"http://ownid.com")]
        [InlineData(false, "https://ownid.com")]
        public void Validate_Valid_Origin(bool isDev, string fido2Url)
        {
            var config = GetValidConfiguration();
            config.AuthenticationMode = AuthenticationModeType.All;

            config.Fido2.Origin = new Uri(fido2Url);
            config.IsDevEnvironment = isDev;

            var result = _validator.Validate(string.Empty, config);

            result.Succeeded.Should().BeTrue();
        }
    }
}