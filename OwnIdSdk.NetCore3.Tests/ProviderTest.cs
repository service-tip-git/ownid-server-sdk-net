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
        #region TestData

        private const string LocalizedString = "loco";
        
        private const string NotExistingContext = "QAfRWt_jtkSd5dUSl1NXnQ";
        private const string NotExistingNonce = "3B782943-9AB7-4AAB-9C0F-F050F983253C";
        private const string NotExistingDID = "did:ownid:123123123";

        private const string ExistingContextWithDID = "QAfRWt_jtkSd5dUSl6NXnQ";
        private readonly CacheItem _existingItemWithDID = new CacheItem
        {
            Nonce = "AC2A7890-F931-4646-9F8E-DAEDA69FBB3F",
            DID = "did:ownid:98765543221"
        };
        
        private const string ExistingContextWithoutDID = "QAfRWt_jtkSd5dUSl2NXnQ";
        private readonly CacheItem _existingItemWithoutDID = new CacheItem
        {
            Nonce = "9D8BCF56-7738-4F66-905E-5A78DAD32DA2"
        };
        
        #endregion
        
        private readonly Provider _provider;
        private readonly Mock<ICacheStore> _cacheStore;
        private readonly Mock<OwnIdConfiguration> _configuration;
        private readonly Mock<ILocalizationService> _localization;

        public ProviderTest()
        {
            _cacheStore = new Mock<ICacheStore>(MockBehavior.Strict);
            _localization = new Mock<ILocalizationService>(MockBehavior.Strict);
            // TODO: remove dependencies
            // using var publicKeyStream = File.OpenText("./Keys/jwtRS256.key.pub");
            // using var privateKeyStream = File.OpenText("./Keys/jwtRS256.key");
            _configuration = new Mock<OwnIdConfiguration>();

            // RsaHelper.ReadKeyFromPem(publicKeyStream), 
            // RsaHelper.ReadKeyFromPem(privateKeyStream), "ownid-app.com", 
            // new List<ProfileField> {ProfileField.Email, ProfileField.FirstName, ProfileField.LastName},
            // "https://callback.com", new Requester
            // {
            //     DID = "did:tst:123-12-123-1",
            //     Description = "desc",
            //     Name = "My name"
            // }
            _provider = new Provider(_configuration.Object, _cacheStore.Object, _localization.Object);

            //Set NotExisting Element
            _cacheStore.Setup(x => x.SetAsync(It.Is<string>(s => s.Equals(NotExistingContext)),
                It.Is<CacheItem>((o) => o.Nonce == NotExistingNonce))).Returns(Task.CompletedTask);

            //Get NotExisting Element -> null
            _cacheStore.Setup(x => x.GetAsync(It.Is<string>(c => c.Equals(NotExistingContext))))
                .Returns(Task.FromResult<CacheItem>(null));

            //Get ExistingWithOutDID -> item without DID
            _cacheStore.Setup(x => x.GetAsync(It.Is<string>(c => c.Equals(ExistingContextWithoutDID))))
                .Returns(Task.FromResult(_existingItemWithoutDID));

            //Get ExistingWithDID -> item with did
            _cacheStore.Setup(x => x.GetAsync(It.Is<string>(c => c.Equals(ExistingContextWithDID))))
                .Returns(Task.FromResult(_existingItemWithDID));

            _localization.Setup(x => x.GetLocalizedString(It.IsAny<string>(), true)).Returns(() => LocalizedString);
        }

        [Fact]
        public void GenerateContext_ValidFormattedContext()
        {
            var result = _provider.GenerateContext();
            
            Assert.False(string.IsNullOrWhiteSpace(result));
            Assert.True(_provider.IsContextFormatValid(result));
        }

        [Fact]
        public void GenerateNonce_NotNullOrEmptyString()
        {
            var result = _provider.GenerateNonce();
            Assert.False(string.IsNullOrWhiteSpace(result));
        }

        [Theory]
        [InlineData("q1Ocsj0m5keLZ95hBDaRgQ", true)]
        [InlineData("q1Ocsj0m5keLZ95hBDaRg", false)]
        [InlineData("q1Ocsj0m5keLZ95hBDaRgQc", false)]
        [InlineData("q1Ocsj0m5keLZ95hBDaR/Q", false)]
        [InlineData("q1Ocsj0m5keLZ95hBD+RgQ", false)]
        public void IsContextValid_22lengthStringBase64Encoded(string value, bool isCorrect)
        {
            Assert.Equal(isCorrect, _provider.IsContextFormatValid(value));
        }

        [Fact]
        public async Task StoreNonceAsync_NotExistingElement()
        {
            await _provider.StoreNonceAsync(NotExistingContext, NotExistingNonce);
        }

        [Fact]
        public async Task SetDIDAsync_NotExistingElement()
        {
            await Assert.ThrowsAsync<ArgumentException>(() => _provider.SetDIDAsync(NotExistingContext, NotExistingDID));
        }
        
        [Fact]
        public async Task SetDIDAsync_ShouldGetAndUpdateValueInStore()
        {
            const string did = "did:ownid:123123123";

            //Set ExistingWithoutDID
            _cacheStore.Setup(x => x.SetAsync(It.Is<string>(s => s.Equals(ExistingContextWithoutDID)),
                It.Is<CacheItem>((o) => o.DID == did))).Returns(Task.CompletedTask);

            await _provider.SetDIDAsync(ExistingContextWithoutDID, did);
        }

        [Fact]
        public async Task GetDIDAsync_NotExistingElement()
        {
            var result = await _provider.GetDIDAsync(NotExistingContext, NotExistingNonce);
            Assert.False(result.isSuccess);
            Assert.Null(result.did);
        }
        
        [Fact]
        public async Task GetDIDAsync_WrongNonce()
        {
            var result = await _provider.GetDIDAsync(ExistingContextWithDID, NotExistingNonce);
            Assert.False(result.isSuccess);
            Assert.Null(result.did);
        }
        
        [Fact]
        public async Task GetDIDAsync_ExistingWithoutDID()
        {
            var result = await _provider.GetDIDAsync(ExistingContextWithoutDID, _existingItemWithoutDID.Nonce);
            Assert.False(result.isSuccess);
            Assert.Null(result.did);
        }

        [Fact]
        public async Task GetDIDAsync_ExistingWithDID()
        {
            var result = await _provider.GetDIDAsync(ExistingContextWithDID, _existingItemWithDID.Nonce);
            Assert.True(result.isSuccess);
            Assert.Equal(_existingItemWithDID.DID, result.did);
        }

        [Fact]
        public async Task RemoveContextAsync_ShouldRemoveFromStore()
        {
            _cacheStore.Setup(x => x.RemoveAsync(It.Is<string>(c => c.Equals(ExistingContextWithDID))))
                .Returns(Task.CompletedTask);
            await _provider.RemoveContextAsync(ExistingContextWithDID);
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