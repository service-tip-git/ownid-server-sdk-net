using System;
using System.Threading.Tasks;
using Moq;
using OwnIdSdk.NetCore3.Configuration;
using OwnIdSdk.NetCore3.Contracts.Jwt;
using OwnIdSdk.NetCore3.Flow;
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
            DID = "did:ownid:98765543221",
            Status = CacheItemStatus.Finished,
        };

        private const string ExistingContextWithoutDID = "QAfRWt_jtkSd5dUSl2NXnQ";

        private readonly CacheItem _existingItemWithoutDID = new CacheItem
        {
            Nonce = "9D8BCF56-7738-4F66-905E-5A78DAD32DA2"
        };

        #endregion

        private readonly OwnIdProvider _ownIdProvider;
        private readonly Mock<ICacheStore> _cacheStore;
        private readonly Mock<OwnIdCoreConfiguration> _configuration;
        private readonly Mock<ILocalizationService> _localization;

        public ProviderTest()
        {
            _cacheStore = new Mock<ICacheStore>(MockBehavior.Strict);
            _localization = new Mock<ILocalizationService>(MockBehavior.Strict);
            _configuration = new Mock<OwnIdCoreConfiguration>();
            _ownIdProvider = new OwnIdProvider(_configuration.Object, _cacheStore.Object, _localization.Object);

            //Set NotExisting Element
            _cacheStore.Setup(x => x.SetAsync(
                    It.Is<string>(s => s.Equals(NotExistingContext))
                    , It.Is<CacheItem>((o) => o.Nonce == NotExistingNonce)
                    , It.IsAny<TimeSpan>()
                )
            ).Returns(Task.CompletedTask);

            //Get NotExisting Element -> null
            _cacheStore.Setup(x => x.GetAsync(It.Is<string>(c => c.Equals(NotExistingContext))))
                .Returns(Task.FromResult<CacheItem>(null));

            //Get ExistingWithOutDID -> item without DID
            _cacheStore.Setup(x => x.GetAsync(It.Is<string>(c => c.Equals(ExistingContextWithoutDID))))
                .Returns(Task.FromResult(_existingItemWithoutDID));

            //Get ExistingWithDID -> item with did
            _cacheStore.Setup(x => x.GetAsync(It.Is<string>(c => c.Equals(ExistingContextWithDID))))
                .Returns(Task.FromResult(_existingItemWithDID));
            _cacheStore.Setup(x => x.RemoveAsync(It.Is<string>(c => c.Equals(ExistingContextWithDID))))
                .Returns(Task.CompletedTask);

            _localization.Setup(x => x.GetLocalizedString(It.IsAny<string>(), true)).Returns(() => LocalizedString);
        }

        [Fact]
        public void GenerateContext_ValidFormattedContext()
        {
            var result = _ownIdProvider.GenerateContext();
            
            Assert.False(string.IsNullOrWhiteSpace(result));
            Assert.True(_ownIdProvider.IsContextFormatValid(result));
        }

        [Fact]
        public void GenerateNonce_NotNullOrEmptyString()
        {
            var result = _ownIdProvider.GenerateNonce();
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
            Assert.Equal(isCorrect, _ownIdProvider.IsContextFormatValid(value));
        }

        [Fact]
        public async Task StoreNonceAsync_NotExistingElement()
        {
            await _ownIdProvider.CreateAuthFlowSessionItemAsync(NotExistingContext, NotExistingNonce, ChallengeType.Login, FlowType.Authorize);
        }

        [Fact]
        public async Task SetDIDAsync_NotExistingElement()
        {
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _ownIdProvider.FinishAuthFlowSessionAsync(NotExistingContext, NotExistingDID));
        }

        [Fact]
        public async Task SetDIDAsync_ShouldGetAndUpdateValueInStore()
        {
            const string did = "did:ownid:123123123";

            //Set ExistingWithoutDID
            _cacheStore.Setup(
                x => x.SetAsync(
                    It.Is<string>(s => s.Equals(ExistingContextWithoutDID))
                    , It.Is<CacheItem>((o) => o.DID == did)
                    , It.IsAny<TimeSpan>()
                )
            ).Returns(Task.CompletedTask);

            await _ownIdProvider.FinishAuthFlowSessionAsync(ExistingContextWithoutDID, did);
        }

        [Fact]
        public async Task GetDIDAsync_NotExistingElement()
        {
            var result = await _ownIdProvider.PopFinishedAuthFlowSessionAsync(NotExistingContext, NotExistingNonce);
            Assert.Null(result);
        }

        [Fact]
        public async Task GetDIDAsync_WrongNonce()
        {
            var result = await _ownIdProvider.PopFinishedAuthFlowSessionAsync(ExistingContextWithDID, NotExistingNonce);
            Assert.Null(result);
        }

        [Fact]
        public async Task GetDIDAsync_ExistingWithoutDID()
        {
            var result =
                await _ownIdProvider.PopFinishedAuthFlowSessionAsync(ExistingContextWithoutDID,
                    _existingItemWithoutDID.Nonce);
            Assert.NotNull(result);
            Assert.NotEqual(CacheItemStatus.Finished, result.Status);
            Assert.Null(result.DID);
        }

        [Fact]
        public async Task GetDIDAsync_ExistingWithDID()
        {
            var result =
                await _ownIdProvider.PopFinishedAuthFlowSessionAsync(ExistingContextWithDID,
                    _existingItemWithDID.Nonce);
            Assert.NotNull(result);
            Assert.Equal(CacheItemStatus.Finished, result.Status);
            Assert.Equal(_existingItemWithDID.DID, result.DID);
        }
    }
}