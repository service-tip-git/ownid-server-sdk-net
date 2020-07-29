using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Moq;
using OwnIdSdk.NetCore3.Extensibility.Configuration;
using OwnIdSdk.NetCore3.Extensibility.Flow;
using OwnIdSdk.NetCore3.Extensibility.Flow.Contracts;
using OwnIdSdk.NetCore3.Extensibility.Providers;
using OwnIdSdk.NetCore3.Flow.Adapters;
using OwnIdSdk.NetCore3.Flow.Commands;
using OwnIdSdk.NetCore3.Services;
using OwnIdSdk.NetCore3.Tests.TestUtils;
using Xunit;

namespace OwnIdSdk.NetCore3.Tests.Flow.Commands
{
    public class CreateFlowCommandTest
    {
        [Theory]
        [InlineData(ChallengeType.Register, false, false, null, FlowType.Authorize)]
        [InlineData(ChallengeType.Register, false, true, null, FlowType.PartialAuthorize)]
        [InlineData(ChallengeType.Login, false, false, null, FlowType.Authorize)]
        [InlineData(ChallengeType.Login, false, true, null, FlowType.PartialAuthorize)]
        [InlineData(ChallengeType.Recover, false, false, "test", FlowType.Recover)]
        [InlineData(ChallengeType.Recover, true, false, "test", FlowType.RecoverWithPin)]
        [InlineData(ChallengeType.Link, false, false, "test", FlowType.Link)]
        [InlineData(ChallengeType.Link, true, false, "test", FlowType.LinkWithPin)]
        public async Task ExecuteAsync_GeneralFlow(ChallengeType challengeType, bool isQr, bool isPartial, string payload, FlowType expectedFlowType)
        {
            var fixture = new Fixture().SetOwnidSpecificSettings();

            var cacheService = fixture.Freeze<Mock<ICacheItemService>>();
            var urlProvider = fixture.Freeze<IUrlProvider>();
            var identitiesProvider = fixture.Freeze<IIdentitiesProvider>();
            var configuration = fixture.Freeze<IOwnIdCoreConfiguration>();
            var linkAdapter = fixture.Freeze<Mock<IAccountLinkHandlerAdapter>>();

            var command = new CreateFlowCommand(cacheService.Object, urlProvider, identitiesProvider,
                configuration, linkAdapter.Object);

            var actual = await command.ExecuteAsync(new GenerateContextRequest
            {
                Type = challengeType,
                IsQr = isQr,
                IsPartial = isPartial,
                Payload = payload
            });

            var context = identitiesProvider.GenerateContext();
            var nonce = identitiesProvider.GenerateNonce();
            var expiration = configuration.CacheExpirationTimeout;
            string did = null;

            //TODO: refactor implementation
            if (challengeType == ChallengeType.Link)
            {
                var capturedPayload = payload;
                linkAdapter.Verify(x=>x.GetCurrentUserIdAsync(capturedPayload), Times.Once);
                did = await linkAdapter.Object.GetCurrentUserIdAsync(payload);
                payload = null;
            }

            cacheService.Verify(x=>x.CreateAuthFlowSessionItemAsync(context, nonce, challengeType, expectedFlowType, did, payload), Times.Once);

            var expected = new GetChallengeLinkResponse(context,
                urlProvider.GetWebAppWithCallbackUrl(urlProvider.GetStartFlowUrl(context)).ToString(), nonce,
                expiration);

            actual.Should().BeEquivalentTo(expected);
        }
    }
}