using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using OwnIdSdk.NetCore3.Extensibility.Flow.Contracts;
using OwnIdSdk.NetCore3.Extensibility.Json;
using OwnIdSdk.NetCore3.Flow.Commands;
using OwnIdSdk.NetCore3.Flow.Interfaces;
using OwnIdSdk.NetCore3.Flow.Steps;
using OwnIdSdk.NetCore3.Web.Attributes;

namespace OwnIdSdk.NetCore3.Web.Middlewares
{
    [RequestDescriptor(BaseRequestFields.Context | BaseRequestFields.RequestToken | BaseRequestFields.ResponseToken)]
    public class CheckUserExistenceMiddleware : BaseMiddleware
    {
        private readonly IFlowRunner _flowRunner;

        public CheckUserExistenceMiddleware(RequestDelegate next, ILogger<CheckUserExistenceMiddleware> logger,
            IFlowRunner flowRunner) : base(next, logger)
        {
            _flowRunner = flowRunner;
        }

        protected override async Task Execute(HttpContext httpContext)
        {
            var request = await OwnIdSerializer.DeserializeAsync<UserExistsRequest>(httpContext.Request.Body);
            
            var commandInput = new CommandInput<UserExistsRequest>(RequestIdentity, GetRequestCulture(httpContext),
                request, ClientDate);
            
            var result = await _flowRunner.RunAsync(commandInput, StepType.CheckUserExistence);
            await Json(httpContext, result, StatusCodes.Status200OK);
        }
    }
}