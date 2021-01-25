using System;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using OwnID.Commands.Pin;
using OwnID.Extensibility.Cache;
using OwnID.Extensibility.Exceptions;
using OwnID.Extensibility.Flow.Contracts;
using OwnID.Services;
using OwnID.Tests.TestUtils;
using Xunit;

namespace OwnID.Tests.Commands.Approval
{
    public class ApproveActionCommandTest
    {
        [Theory]
        [AutoMoqData]
        public async Task ExecuteAsync_Fail_EmptyContext(Mock<ICacheItemRepository> cacheItemRepository,
            ApproveActionRequest request)
        {
            const string errorMessage = "Context and nonce are required";
            request.Context = null;
            var command = new ApproveActionCommand(cacheItemRepository.Object);
            await command.Invoking(x => x.ExecuteAsync(request)).Should().ThrowAsync<CommandValidationException>()
                .WithMessage(errorMessage);
            request.Context = string.Empty;
            await command.Invoking(x => x.ExecuteAsync(request)).Should().ThrowAsync<CommandValidationException>()
                .WithMessage(errorMessage);
        }

        [Theory]
        [AutoMoqData]
        public async Task ExecuteAsync_Fail_EmptyNonce(Mock<ICacheItemRepository> cacheItemRepository,
            ApproveActionRequest request)
        {
            const string errorMessage = "Context and nonce are required";
            request.Nonce = null;
            var command = new ApproveActionCommand(cacheItemRepository.Object);
            await command.Invoking(x => x.ExecuteAsync(request)).Should().ThrowAsync<CommandValidationException>()
                .WithMessage(errorMessage);
            request.Nonce = string.Empty;
            await command.Invoking(x => x.ExecuteAsync(request)).Should().ThrowAsync<CommandValidationException>()
                .WithMessage(errorMessage);
        }

        [Theory]
        [AutoMoqData]
        public async Task ExecuteAsync_Success(Mock<ICacheItemRepository> cacheItemRepository,
            ApproveActionRequest request)
        {
            var command = new ApproveActionCommand(cacheItemRepository.Object);
            await command.ExecuteAsync(request);
            cacheItemRepository.Verify(x => x.UpdateAsync(request.Context, It.IsAny<Action<CacheItem>>(), null),
                Times.Once);
        }
    }
}