using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using OwnIdSdk.NetCore3.Extensibility.Flow.Contracts;
using OwnIdSdk.NetCore3.Extensibility.Json;
using OwnIdSdk.NetCore3.Flow.Commands;

namespace OwnIdSdk.NetCore3.Web.Middlewares
{
    public class GetChallengeStatusMiddleware : BaseMiddleware
    {
        private readonly GetStatusCommand _getStatusCommand;

        public GetChallengeStatusMiddleware(RequestDelegate next, GetStatusCommand getStatusCommand,
            ILogger<GetChallengeStatusMiddleware> logger) : base(next, logger)
        {
            _getStatusCommand = getStatusCommand;
        }

        protected override async Task Execute(HttpContext context)
        {
            List<GetStatusRequest> request;
            try
            {
                request = await OwnIdSerializer.DeserializeAsync<List<GetStatusRequest>>(context.Request.Body);
            }
            catch
            {
                BadRequest(context.Response);
                return;
            }

            var result = await _getStatusCommand.ExecuteAsync(request);

            context.Response.StatusCode = StatusCodes.Status200OK;
            context.Response.ContentType = "application/json";

            // TODO: remove after web ui sdk changes enums as strings
            await context.Response.WriteAsync(JsonSerializer.Serialize<object>(result, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                IgnoreNullValues = true
            }));

            // TODO: uncomment after web ui sdk changes enums as strings
            // await Json(context, result, StatusCodes.Status200OK, false);
        }
    }
}