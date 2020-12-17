using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using OwnID.Extensibility.Flow.Contracts;
using OwnID.Extensibility.Json;
using OwnID.Flow.Commands;

namespace OwnID.Web.Middlewares
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

        protected override async Task ExecuteAsync(HttpContext httpContext)
        {
            var request = await OwnIdSerializer.DeserializeAsync<GenerateContextRequest>(httpContext.Request.Body);

            var result = await _createFlowCommand.ExecuteAsync(request);
            await Json(httpContext, result, StatusCodes.Status200OK, false);
        }
    }
}