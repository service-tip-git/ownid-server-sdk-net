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
    [RequestDescriptor(BaseRequestFields.Context | BaseRequestFields.RequestToken | BaseRequestFields.ResponseToken)]
    public class Fido2Middleware : BaseMiddleware
    {
        private readonly IFlowRunner _flowRunner;

        public Fido2Middleware(RequestDelegate next, IFlowRunner flowRunner, ILogger<Fido2Middleware> logger) : base(
            next, logger)
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