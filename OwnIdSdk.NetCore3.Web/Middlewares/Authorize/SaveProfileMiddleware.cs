using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using OwnIdSdk.NetCore3.Extensibility.Flow.Contracts;
using OwnIdSdk.NetCore3.Extensibility.Flow.Contracts.Jwt;
using OwnIdSdk.NetCore3.Flow;
using OwnIdSdk.NetCore3.Flow.Commands;
using OwnIdSdk.NetCore3.Flow.Interfaces;
using OwnIdSdk.NetCore3.Web.Attributes;

namespace OwnIdSdk.NetCore3.Web.Middlewares.Authorize
{
    [RequestDescriptor(BaseRequestFields.Context | BaseRequestFields.RequestToken |
                       BaseRequestFields.ResponseToken)]
    public class SaveProfileMiddleware : BaseMiddleware
    {
        private readonly IFlowRunner _flowRunner;

        public SaveProfileMiddleware(RequestDelegate next, IFlowRunner flowRunner,
            ILogger<SaveProfileMiddleware> logger) : base(next, logger)
        {
            _flowRunner = flowRunner;
        }

        protected override async Task Execute(HttpContext httpContext)
        {
            var jwtContainer = await GetRequestJwtContainerAsync(httpContext);
            var result = await _flowRunner.RunAsync(new CommandInput<JwtContainer>
            {
                Context = RequestIdentity.Context,
                RequestToken = RequestIdentity.RequestToken,
                ResponseToken = RequestIdentity.RequestToken,
                CultureInfo = GetRequestCulture(httpContext),
                Data = jwtContainer
            }, StepType.Authorize);

            await Json(httpContext, result, StatusCodes.Status200OK);
        }
    }
}