using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using OwnID.Extensibility.Flow.Contracts;
using OwnID.Extensibility.Flow.Contracts.Jwt;
using OwnID.Flow.Commands;
using OwnID.Flow.Interfaces;
using OwnID.Flow.Steps;
using OwnID.Web.Attributes;

namespace OwnID.Web.Middlewares.Recover
{
    [RequestDescriptor(BaseRequestFields.Context | BaseRequestFields.RequestToken | BaseRequestFields.ResponseToken)]
    public class SaveAccountPublicKeyMiddleware : BaseMiddleware
    {
        private readonly IFlowRunner _flowRunner;

        public SaveAccountPublicKeyMiddleware(RequestDelegate next, IFlowRunner flowRunner,
            ILogger<SaveAccountPublicKeyMiddleware> logger, StopFlowCommand stopFlowCommand) : base(next, logger,
            stopFlowCommand)
        {
            _flowRunner = flowRunner;
        }

        protected override async Task Execute(HttpContext httpContext)
        {
            var jwtContainer = await GetRequestJwtContainerAsync(httpContext);
            var result = await _flowRunner.RunAsync(
                new CommandInput<JwtContainer>(RequestIdentity, GetRequestCulture(httpContext), jwtContainer,
                    ClientDate),
                StepType.Recover);
            await Json(httpContext, result, StatusCodes.Status200OK);
        }
    }
}