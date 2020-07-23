using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using OwnIdSdk.NetCore3.Extensibility.Flow.Contracts;
using OwnIdSdk.NetCore3.Flow;
using OwnIdSdk.NetCore3.Flow.Commands;
using OwnIdSdk.NetCore3.Flow.Interfaces;
using OwnIdSdk.NetCore3.Flow.Steps;
using OwnIdSdk.NetCore3.Web.Attributes;

namespace OwnIdSdk.NetCore3.Web.Middlewares
{
    [RequestDescriptor(BaseRequestFields.Context | BaseRequestFields.RequestToken)]
    public class StartFlowMiddleware : BaseMiddleware
    {
        private readonly IFlowRunner _flowRunner;

        public StartFlowMiddleware(RequestDelegate next, IFlowRunner flowRunner,
            ILogger<StartFlowMiddleware> logger) : base(next, logger)
        {
            _flowRunner = flowRunner;
        }

        protected override async Task Execute(HttpContext httpContext)
        {
            var result = await _flowRunner.RunAsync(new CommandInput
            {
                Context = RequestIdentity.Context,
                RequestToken = RequestIdentity.RequestToken,
                CultureInfo = GetRequestCulture(httpContext)
            }, StepType.Starting);

            await Json(httpContext, result, StatusCodes.Status200OK);
        }
    }
}