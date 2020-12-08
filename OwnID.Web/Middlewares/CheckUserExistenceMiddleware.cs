using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using OwnID.Extensibility.Flow.Contracts;
using OwnID.Extensibility.Json;
using OwnID.Flow.Commands;
using OwnID.Flow.Interfaces;
using OwnID.Flow.Steps;
using OwnID.Web.Attributes;

namespace OwnID.Web.Middlewares
{
    [RequestDescriptor(BaseRequestFields.Context | BaseRequestFields.RequestToken | BaseRequestFields.ResponseToken)]
    public class CheckUserExistenceMiddleware : BaseMiddleware
    {
        private readonly IFlowRunner _flowRunner;

        public CheckUserExistenceMiddleware(RequestDelegate next, ILogger<CheckUserExistenceMiddleware> logger,
            IFlowRunner flowRunner, StopFlowCommand stopFlowCommand) : base(next, logger, stopFlowCommand)
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