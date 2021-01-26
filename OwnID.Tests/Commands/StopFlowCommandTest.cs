using System;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Moq;
using OwnID.Commands;
using OwnID.Extensibility.Cache;
using OwnID.Extensibility.Configuration;
using OwnID.Services;
using OwnID.Tests.TestUtils;
using Xunit;

namespace OwnID.Tests.Commands
{
    public class StopFlowCommandTest
    {
        [Fact]
        public async Task Execute_Test()
        {
            var fixture = new Fixture().SetOwnidSpecificSettings();
            var cacheStore = new Mock<ICacheStore>();
            var cacheItemRepository = new Mock<CacheItemRepository>(() =>
                new CacheItemRepository(cacheStore.Object, fixture.Create<IOwnIdCoreConfiguration>()));
            var command = new StopFlowCommand(cacheItemRepository.Object);
            var context = fixture.Create<string>();
            var error = fixture.Create<string>();
            var cacheItem = fixture.Create<CacheItem>();
            cacheItem.Context = context;

            cacheStore.Setup(x => x.GetAsync(It.Is<string>(i => i == context))).Returns(Task.FromResult(cacheItem));
            cacheStore.Setup(x =>
                x.SetAsync(It.Is<string>(i => i == context), It.IsAny<CacheItem>(), It.IsAny<TimeSpan>()));

            await command.ExecuteAsync(context, error);

            cacheItem.Error.Should().BeEquivalentTo(error);
            cacheItem.Status.Should().Be(CacheItemStatus.Finished);
        }
    }
}