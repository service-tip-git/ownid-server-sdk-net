using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using OwnIdSdk.NetCore3.Extensibility.Flow.Contracts;
using OwnIdSdk.NetCore3.Extensibility.Flow.Contracts.Jwt;
using OwnIdSdk.NetCore3.Flow.Commands;
using OwnIdSdk.NetCore3.Flow.Interfaces;
using OwnIdSdk.NetCore3.Flow.Steps;
using OwnIdSdk.NetCore3.Web.Attributes;

namespace OwnIdSdk.NetCore3.Web.Middlewares.Recover
{
    [RequestDescriptor(BaseRequestFields.Context | BaseRequestFields.RequestToken | BaseRequestFields.ResponseToken)]
    public class SaveAccountPublicKeyMiddleware : BaseMiddleware
    {
        private readonly IFlowRunner _flowRunner;

        public SaveAccountPublicKeyMiddleware(RequestDelegate next, IFlowRunner flowRunner,
            ILogger<SaveAccountPublicKeyMiddleware> logger) : base(next, logger)
        {
            _flowRunner = flowRunner;
        }

        protected override async Task Execute(HttpContext httpContext)
        {
            var jwtContainer = await GetRequestJwtContainerAsync(httpContext);
            var result = await _flowRunner.RunAsync(
                new CommandInput<JwtContainer>(RequestIdentity, GetRequestCulture(httpContext), jwtContainer),
                StepType.Recover);
            await Json(httpContext, result, StatusCodes.Status200OK);
        }
    }
}