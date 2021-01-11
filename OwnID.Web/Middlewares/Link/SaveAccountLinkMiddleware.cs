using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using OwnID.Extensibility.Flow.Contracts;
using OwnID.Extensibility.Flow.Contracts.Jwt;
using OwnID.Flow;
using OwnID.Flow.Interfaces;
using OwnID.Web.Attributes;

namespace OwnID.Web.Middlewares.Link
{
    [RequestDescriptor(BaseRequestFields.Context | BaseRequestFields.RequestToken | BaseRequestFields.ResponseToken)]
    public class SaveAccountLinkMiddleware : BaseMiddleware
    {
        private readonly IFlowRunner _flowRunner;

        public SaveAccountLinkMiddleware(RequestDelegate next, IFlowRunner flowRunner,
            ILogger<SaveAccountLinkMiddleware> logger) : base(next, logger)
        {
            _flowRunner = flowRunner;
        }

        protected override async Task ExecuteAsync(HttpContext httpContext)
        {
            var jwtContainer = await GetRequestJwtContainerAsync(httpContext);
            var result = await _flowRunner.RunAsync(
                new TransitionInput<JwtContainer>(RequestIdentity, GetRequestCulture(httpContext), jwtContainer,
                    ClientDate),
                StepType.Link);

            SetCookies(httpContext.Response, result);
            await JsonAsync(httpContext, result, StatusCodes.Status200OK);
        }
    }
}