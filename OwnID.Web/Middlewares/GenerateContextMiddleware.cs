using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using OwnID.Commands;
using OwnID.Extensibility.Flow.Contracts;
using OwnID.Extensibility.Json;

namespace OwnID.Web.Middlewares
{
    public class GenerateContextMiddleware : BaseMiddleware
    {
        private readonly CreateFlowCommand _createFlowCommand;

        public GenerateContextMiddleware(RequestDelegate next, CreateFlowCommand createFlowCommand,
            ILogger<GenerateContextMiddleware> logger) : base(next, logger)
        {
            _createFlowCommand = createFlowCommand;
        }

        protected override async Task ExecuteAsync(HttpContext httpContext)
        {
            var request = await OwnIdSerializer.DeserializeAsync<GenerateContextRequest>(httpContext.Request.Body);

            var result = await _createFlowCommand.ExecuteAsync(request);
            await JsonAsync(httpContext, result, StatusCodes.Status200OK, false);
        }
    }
}