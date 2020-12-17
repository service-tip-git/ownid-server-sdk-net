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
    public class InternalConnectionRecoveryMiddleware : BaseMiddleware
    {
        private readonly IFlowRunner _flowRunner;

        public InternalConnectionRecoveryMiddleware(RequestDelegate next,
            ILogger<InternalConnectionRecoveryMiddleware> logger, IFlowRunner flowRunner,
            StopFlowCommand stopFlowCommand) : base(next, logger, stopFlowCommand)
        {
            _flowRunner = flowRunner;
        }

        protected override async Task ExecuteAsync(HttpContext httpContext)
        {
            var input = new CommandInput(RequestIdentity, GetRequestCulture(httpContext), ClientDate);

            var result = await _flowRunner.RunAsync(input, StepType.InternalConnectionRecovery);

            await Json(httpContext, result, StatusCodes.Status200OK);
        }
    }
}