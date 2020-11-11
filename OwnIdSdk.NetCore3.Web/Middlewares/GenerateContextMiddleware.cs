using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using OwnIdSdk.NetCore3.Extensibility.Flow.Contracts;
using OwnIdSdk.NetCore3.Extensibility.Json;
using OwnIdSdk.NetCore3.Flow.Commands;

namespace OwnIdSdk.NetCore3.Web.Middlewares
{
    public class GenerateContextMiddleware : BaseMiddleware
    {
        private readonly CreateFlowCommand _createFlowCommand;

        public GenerateContextMiddleware(RequestDelegate next, CreateFlowCommand createFlowCommand,
            ILogger<GenerateContextMiddleware> logger, StopFlowCommand stopFlowCommand) : base(next, logger,
            stopFlowCommand)
        {
            _createFlowCommand = createFlowCommand;
        }

        protected override async Task Execute(HttpContext httpContext)
        {
            var request = await OwnIdSerializer.DeserializeAsync<GenerateContextRequest>(httpContext.Request.Body);

            var result = await _createFlowCommand.ExecuteAsync(request);
            await Json(httpContext, result, StatusCodes.Status200OK, false);
        }
    }
}