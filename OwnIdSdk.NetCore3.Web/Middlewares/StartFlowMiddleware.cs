using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using OwnIdSdk.NetCore3.Extensibility.Flow.Contracts;
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

        public StartFlowMiddleware(
            RequestDelegate next,
            IFlowRunner flowRunner,
            ILogger<StartFlowMiddleware> logger
        ) : base(next, logger)
        {
            _flowRunner = flowRunner;
        }

        protected override async Task Execute(HttpContext httpContext)
        {
            using var requestBodyStreamReader = new StreamReader(httpContext.Request.Body);
            var requestBody = await requestBodyStreamReader.ReadToEndAsync();

            var commandInput = new CommandInput<string>(RequestIdentity, GetRequestCulture(httpContext), requestBody,
                ClientDate);

            var commandResult = await _flowRunner.RunAsync(commandInput, StepType.Starting);

            await Json(httpContext, commandResult, StatusCodes.Status200OK);
        }
    }
}