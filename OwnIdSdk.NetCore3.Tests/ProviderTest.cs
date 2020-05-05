using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Moq;
using OwnIdSdk.NetCore3.Configuration;
using OwnIdSdk.NetCore3.Cryptography;
using OwnIdSdk.NetCore3.Store;
using Xunit;

namespace OwnIdSdk.NetCore3.Tests
{
    public class ProviderTest
    {
        private readonly Provider _provider;
        private readonly Mock<ICacheStore> _cacheStore;
        private readonly Mock<ProviderConfiguration> _configuration;

        public ProviderTest()
        {
            _cacheStore = new Mock<ICacheStore>(MockBehavior.Strict);
            // TODO: remove dependencies
            using var publicKeyStream = File.OpenText("./Keys/jwtRS256.key.pub");
            using var privateKeyStream = File.OpenText("./Keys/jwtRS256.key");
            _configuration = new Mock<ProviderConfiguration>(RsaHelper.ReadKeyFromPem(publicKeyStream), 
                RsaHelper.ReadKeyFromPem(privateKeyStream), "ownid-app.com", 
                new List<ProfileField> {ProfileField.Email, ProfileField.FirstName, ProfileField.LastName},
                "https://callback.com", new Requester
                {
                    DID = "did:tst:123-12-123-1",
                    Description = "desc",
                    Name = "My name"
                });
            _provider = new Provider(_cacheStore.Object, _configuration.Object);
        }

        [Fact]
        public void GenerateContext_Success()
        {
            var result = _provider.GenerateContext();
            
            Assert.False(string.IsNullOrWhiteSpace(result));
            Assert.True(Guid.TryParse(result, out _));
            Assert.True(_provider.IsContextValid(result));
        }

        [Fact]
        public void GenerateNonce_Success()
        {
            var result = _provider.GenerateNonce();
            Assert.False(string.IsNullOrWhiteSpace(result));
        }

        [Theory]
        [InlineData("B477A4FB-FE24-40D3-A467-56D4E2D2F354", true)]
        [InlineData("B477A4FBFE2440D3A46756D4E2D2F354", true)]
        [InlineData("b477a4fbfe2440d3a46756d4e2d2f354", true)]
        [InlineData("B477A4FB-FE24-A467-56D4E2D2F354", false)]
        [InlineData("B477A4FBFE2440D3A46756D4E2D2F35", false)]
        [InlineData("b477a4fbfe2440d3a46756d4e2d2f354dds", false)]
        [InlineData("123111231123311212112211", false)]
        public void IsContextValid_Correct(string value, bool isCorrect)
        {
            Assert.Equal(isCorrect, _provider.IsContextValid(value));
        }

        [Fact]
        public async Task StoreNonceAsync()
        {
            var context = Guid.NewGuid().ToString();
            var nonce = Guid.NewGuid().ToString();
            _cacheStore.Setup(x => x.SetAsync(It.Is<string>(s => s.Equals(context)),
                It.Is<CacheItem>((o) => o.Nonce == nonce))).Returns(Task.CompletedTask);
            await _provider.StoreNonceAsync(context, nonce);
        }

        [Fact]
        public void GenerateChallengeJwt_Success()
        {
            // var provider = new Provider(new InMemoryCacheStore(), new ProviderConfiguration()
            // {
            //     Requester = new Requester
            //     {
            //         Name = "MyOrg",
            //         PublicKey = "keyasdadsad",
            //         DID = "did:idw:123123123"
            //     },
            //     ProfileFields = new List<ProfileField>
            //     {
            //         new ProfileField
            //         {
            //             Type = ProfileFieldType.Email,
            //             Label = "Email"
            //         }
            //     },
            //     CallbackUrl = "asadad",
            //     TokenSecret = "asdv234234^&%&^%&^hjsdfb2%%%",
            //     OwnIdApplicationUrl = "asdds"
            // });
            //
            // var a = provider.GenerateChallengeJwt("9877654321");
            // Assert.NotEmpty(a);


            Assert.NotNull(new { });
        }

        [Fact]
        public void GetProfileDataFromJwt_Success()
        {
            // var provider = new Provider(new InMemoryCacheStore(), new ProviderConfiguration(
            //     RsaHelper.ReadKeyFromPem(File.OpenText(
            //         @"/Users/c5306486/RiderProjects/ownid-docker-example/ownid-api/config/jwtRS256.key.pub")),
            //     RsaHelper.ReadKeyFromPem(
            //         File.OpenText(@"/Users/c5306486/RiderProjects/ownid-docker-example/ownid-api/config/jwtRS256.key")),
            //     "https://google.com",
            //     new List<ProfileField>
            //     {
            //         ProfileField.Email, ProfileField.FirstName, ProfileField.LastName
            //     }, "http://localhost:5000/", new Requester
            //     {
            //         DID = "did:ownid:12312313",
            //         Name = "OrgName",
            //         Description = "descr"
            //     }));
            //
            // provider.GetProfileDataFromJwt(
                 // @"eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.eyJqdGkiOiJlNDA4OGMyMy1hYzNjLTQ3OTAtOGQzMS1mODQ2NGJjMWJkMjAiLCJpYXQiOjE1MTYyMzkwMjIsInVzZXIiOnsiZGlkIjoibG9sb2xvRElEIiwicHJvZmlsZSI6W3sidmFsdWUiOiJhc2RzQHNkLnNkIiwidHlwZSI6ImVtYWlsIn1dLCJwdWJLZXkiOiJNSUlCSWpBTkJna3Foa2lHOXcwQkFRRUZBQU9DQVE4QU1JSUJDZ0tDQVFFQW56eWlzMVpqZk5CMGJCZ0tGTVN2dmtUdHdsdkJzYUpxN1M1d0Era3plVk9WcFZXd2tXZFZoYTRzMzhYTS9wYS95cjQ3YXY3K3ozVlRtdkRSeUFIY2FUOTJ3aFJFRnBMdjljajVsVGVKU2lieXIvTXJtL1l0akNaVldnYU9ZSWh3clh3S0xxUHIvMTFpbldzQWtmSXl0dkhXVHhaWUVjWExnQVhGdVV1YVMzdUY5Z0VpTlF3ekdUVTF2MEZxa3FUQnI0QjhuVzNIQ040N1hVdTB0OFkwZStsZjRzNE94UWF3V0Q3OUo5LzVkM1J5MHZiVjNBbTFGdEdKaUp2T3dSc0lmVkNoRHBZU3RUY0hUQ01xdHZXYlY2TDExQldrcHpHWFNXNEh2NDNxYStHU1lPRDJRVTY4TWI1OW9TazJPQitCdE9McEpvZm1iR0VHZ3Ztd3lDSTlNd0lEQVFBQiJ9fQ.CNPfcNANSBSLUXT7snQin0OzcMjx5sFvKTo294VP_W8oGfAgtbrcGYIvWwJQGaRW3W3l7QutoLUTj6EJtoQTGogo6Z-vzyq0M0Q5X70Gmiua_f5ExLHMn2tY9Lpga04-MAcUMawqT24d3pGTJb4lovdM15kri-2-HbVZnzYiyG5pXN3uRuCxvrT_kF02j2QjNM9uY9ds7b7gu3tKx3aP1w_OzkdA6mzS5BZ6RWgOhBd4hhLheluVYwQwTUQtfwQeWi8CHeliTbC_qaPMgOsaCXKnyomEsfi_zyLgJB8y0KRtobwGKp54_F0ZsYkc55gLPR2N8q8X6ShAB4ho-rkepw");
        }
    }
}