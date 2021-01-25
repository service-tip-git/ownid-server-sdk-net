using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Moq;
using OwnID.Commands;
using OwnID.Extensibility.Cache;
using OwnID.Extensibility.Configuration;
using OwnID.Extensibility.Flow;
using OwnID.Extensibility.Flow.Abstractions;
using OwnID.Extensibility.Flow.Contracts;
using OwnID.Extensibility.Flow.Contracts.Link;
using OwnID.Extensibility.Providers;
using OwnID.Services;
using OwnID.Tests.TestUtils;
using Xunit;

namespace OwnID.Tests.Commands
{
    public class CreateFlowCommandTest
    {
        [Theory]
        [InlineData(ChallengeType.Register, false, false, null, false, FlowType.Authorize, Skip = "Flow is abandoned")]
        [InlineData(ChallengeType.Register, false, true, null, false, FlowType.PartialAuthorize)]
        [InlineData(ChallengeType.Register, false, true, null, true, FlowType.PartialAuthorize)]
        [InlineData(ChallengeType.Login, false, false, null, false, FlowType.Authorize, Skip = "Flow is abandoned")]
        [InlineData(ChallengeType.Login, false, true, null, false, FlowType.PartialAuthorize)]
        [InlineData(ChallengeType.Login, false, true, null, true, FlowType.PartialAuthorize)]
        [InlineData(ChallengeType.Recover, false, false, "test", false, FlowType.Recover)]
        [InlineData(ChallengeType.Recover, false, false, "test", true, FlowType.Recover)]
        [InlineData(ChallengeType.Recover, true, false, "test", false, FlowType.RecoverWithPin)]
        [InlineData(ChallengeType.Recover, true, false, "test", true, FlowType.RecoverWithPin)]
        [InlineData(ChallengeType.Link, false, false, "test", false, FlowType.Link)]
        [InlineData(ChallengeType.Link, false, false, "test", true, FlowType.Link)]
        [InlineData(ChallengeType.Link, true, false, "test", false, FlowType.LinkWithPin)]
        [InlineData(ChallengeType.Link, true, false, "test", true, FlowType.LinkWithPin)]
        public async Task ExecuteAsync_GeneralFlow(ChallengeType challengeType, bool isQr, bool isPartial,
            string payload, bool fido2Enabled, FlowType expectedFlowType)
        {
            var fixture = new Fixture().SetOwnidSpecificSettings();

            var cacheService = fixture.Freeze<Mock<ICacheItemRepository>>();
            var urlProvider = fixture.Freeze<IUrlProvider>();
            var identitiesProvider = fixture.Freeze<IIdentitiesProvider>();
            var configuration = fixture.Freeze<IOwnIdCoreConfiguration>();
            var linkAdapter = fixture.Freeze<Mock<IAccountLinkHandler>>();
            var language = fixture.Freeze<string>();

            configuration.TFAEnabled = fido2Enabled;
            configuration.MaximumNumberOfConnectedDevices = 99;

            var did = fixture.Create<string>();
            linkAdapter.Setup(x => x.GetCurrentUserLinkStateAsync(It.IsAny<string>()))
                .ReturnsAsync(new LinkState(did, 1));

            var command = new CreateFlowCommand(cacheService.Object, urlProvider, identitiesProvider,
                configuration, linkAdapter.Object);

            var actual = await command.ExecuteAsync(new GenerateContextRequest
            {
                Type = challengeType,
                IsQr = isQr,
                IsPartial = isPartial,
                Payload = payload,
                Language = language
            });

            var context = identitiesProvider.GenerateContext();
            var nonce = identitiesProvider.GenerateNonce();
            var expiration = configuration.CacheExpirationTimeout;

            //TODO: refactor implementation
            if (challengeType == ChallengeType.Link)
            {
                var capturedPayload = payload;

                linkAdapter.Verify(x => x.GetCurrentUserLinkStateAsync(capturedPayload), Times.Once);
                payload = null;
            }
            else
            {
                did = null;
            }

            cacheService.Verify(
                x => x.CreateAsync(
                    It.Is<CacheItem>(y => y.Context == context && y.Nonce == nonce && y.FlowType == expectedFlowType),
                    null), Times.Once);

            var url = urlProvider.GetWebAppSignWithCallbackUrl(urlProvider.GetStartFlowUrl(context), language);
            var expected = new GetChallengeLinkResponse(context, url.ToString(), nonce, expiration, false);

            actual.Should().BeEquivalentTo(expected);
        }
    }
}