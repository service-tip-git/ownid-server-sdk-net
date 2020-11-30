using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using OwnID.Extensibility.Flow.Contracts;
using OwnID.Flow.Commands;
using OwnID.Flow.Interfaces;
using OwnID.Flow.Steps;
using OwnID.Web.Attributes;

namespace OwnID.Web.Middlewares
{
    [RequestDescriptor(BaseRequestFields.Context | BaseRequestFields.RequestToken | BaseRequestFields.ResponseToken)]
    public class Fido2Middleware : BaseMiddleware
    {
        private readonly IFlowRunner _flowRunner;

        public Fido2Middleware(RequestDelegate next, IFlowRunner flowRunner, ILogger<Fido2Middleware> logger,
            StopFlowCommand stopFlowCommand) : base(next, logger, stopFlowCommand)
        {
            _flowRunner = flowRunner;
        }

        protected override async Task Execute(HttpContext httpContext)
        {
            using var bodyReader = new StreamReader(httpContext.Request.Body);
            var bodyStr = await bodyReader.ReadToEndAsync();

            var result = await _flowRunner.RunAsync(
                new CommandInput<string>(RequestIdentity, GetRequestCulture(httpContext), bodyStr, ClientDate),
                StepType.Fido2Authorize);

            await Json(httpContext, result, StatusCodes.Status200OK);
        }
    }
}