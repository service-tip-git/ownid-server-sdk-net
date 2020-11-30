using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using OwnID.Extensibility.Flow.Contracts.MagicLink;
using OwnID.Extensibility.Json;
using OwnID.Flow.Commands;
using OwnID.Flow.Commands.MagicLink;

namespace OwnID.Web.Middlewares.MagicLink
{
    public class ExchangeMagicLinkMiddleware : BaseMiddleware
    {
        private readonly ExchangeMagicLinkCommand _exchangeMagicLinkCommand;

        public ExchangeMagicLinkMiddleware(RequestDelegate next, ILogger<ExchangeMagicLinkMiddleware> logger,
            StopFlowCommand stopFlowCommand, ExchangeMagicLinkCommand exchangeMagicLinkCommand) : base(next, logger,
            stopFlowCommand)
        {
            _exchangeMagicLinkCommand = exchangeMagicLinkCommand;
        }

        protected override async Task Execute(HttpContext httpContext)
        {
            var request = await OwnIdSerializer.DeserializeAsync<ExchangeMagicLinkRequest>(httpContext.Request.Body);
            await Json(httpContext, await _exchangeMagicLinkCommand.ExecuteAsync(request),
                StatusCodes.Status200OK);
        }
    }
}