using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using OwnID.Flow.Commands;
using OwnID.Flow.Commands.MagicLink;

namespace OwnID.Web.Middlewares.MagicLink
{
    public class SendMagicLinkMiddleware : BaseMiddleware
    {
        private readonly SendMagicLinkCommand _sendMagicLinkCommand;

        public SendMagicLinkMiddleware(RequestDelegate next, ILogger<SendMagicLinkMiddleware> logger,
            StopFlowCommand stopFlowCommand, SendMagicLinkCommand sendMagicLinkCommand) : base(next, logger,
            stopFlowCommand)
        {
            _sendMagicLinkCommand = sendMagicLinkCommand;
        }

        protected override async Task ExecuteAsync(HttpContext httpContext)
        {
            var email = httpContext.Request.Query["email"];

            await Json(httpContext, await _sendMagicLinkCommand.ExecuteAsync(email), StatusCodes.Status200OK);
        }
    }
}