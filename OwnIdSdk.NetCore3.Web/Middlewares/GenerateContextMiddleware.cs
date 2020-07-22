using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using OwnIdSdk.NetCore3.Extensibility.Flow.Contracts;
using OwnIdSdk.NetCore3.Flow.Commands;

namespace OwnIdSdk.NetCore3.Web.Middlewares
{
    public class GenerateContextMiddleware : BaseMiddleware
    {
        private readonly CreateFlowCommand _createFlowCommand;

        public GenerateContextMiddleware(RequestDelegate next, CreateFlowCommand createFlowCommand,
            ILogger<GenerateContextMiddleware> logger) : base(next, logger)
        {
            _createFlowCommand = createFlowCommand;
        }

        protected override async Task Execute(HttpContext httpContext)
        {
            var request = await JsonSerializer.DeserializeAsync<GenerateContextRequest>(httpContext.Request.Body,
                new JsonSerializerOptions
                {
                    Converters = {new JsonStringEnumConverter()}
                });

            var result = await _createFlowCommand.ExecuteAsync(request);
            await Json(httpContext, result, StatusCodes.Status200OK, false);
        }
    }
}