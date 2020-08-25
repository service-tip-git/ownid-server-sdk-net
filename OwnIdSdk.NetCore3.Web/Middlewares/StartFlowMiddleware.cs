using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using OwnIdSdk.NetCore3.Extensibility.Flow;
using OwnIdSdk.NetCore3.Extensibility.Flow.Contracts;
using OwnIdSdk.NetCore3.Extensibility.Flow.Contracts.Fido2;
using OwnIdSdk.NetCore3.Extensibility.Json;
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
            using var streamReader = new StreamReader(httpContext.Request.Body);
            var requestType = await GetRequestTypeAsync(streamReader);
            httpContext.Request.Body.Seek(0, SeekOrigin.Begin);

            var result = requestType switch
            {
                RequestType.Default => await ProcessStartRequestAsync(httpContext),
                RequestType.Fido2Register => await ProcessFido2RegisterRequestAsync(httpContext),
                RequestType.Fido2Login => await ProcessFido2LoginRequestAsync(httpContext),
                _ => throw new ArgumentOutOfRangeException()
            };

            await Json(httpContext, result, StatusCodes.Status200OK);
        }

        private async Task<ICommandResult> ProcessStartRequestAsync(HttpContext httpContext)
        {
            return await _flowRunner.RunAsync(
                new CommandInput(
                    RequestIdentity,
                    GetRequestCulture(httpContext)),
                StepType.Starting);
        }

        private async Task<ICommandResult> ProcessFido2LoginRequestAsync(HttpContext httpContext)
        {
            var fido2LoginRequest =
                await OwnIdSerializer.DeserializeAsync<Fido2LoginRequest>(httpContext.Request.Body);

            var commandInput = new CommandInput<Fido2LoginRequest>(
                RequestIdentity,
                GetRequestCulture(httpContext),
                fido2LoginRequest);

            return await _flowRunner.RunAsync(commandInput, StepType.Fido2Login);
        }

        private async Task<ICommandResult> ProcessFido2RegisterRequestAsync(HttpContext httpContext)
        {
            httpContext.Request.Body.Seek(0, SeekOrigin.Begin);
            var fido2RegisterRequest =
                await OwnIdSerializer.DeserializeAsync<Fido2RegisterRequest>(httpContext.Request.Body);

            var commandInput = new CommandInput<Fido2RegisterRequest>(
                RequestIdentity,
                GetRequestCulture(httpContext),
                fido2RegisterRequest);

            return await _flowRunner.RunAsync(commandInput, StepType.Fido2Register);
        }

        private async Task<RequestType> GetRequestTypeAsync(TextReader reader)
        {
            var requestBody = await reader.ReadToEndAsync();

            JsonDocument json;
            try
            {
                json = JsonDocument.Parse(requestBody);
            }
            catch
            {
                return RequestType.Default;
            }

            if (json == null
                || !json.RootElement.TryGetProperty("fido2", out var fido2Element)
                || !fido2Element.TryGetProperty("type", out var typeElement)
            )
            {
                return RequestType.Default;
            }

            var type = typeElement.GetString();
            return type switch
            {
                "r" => RequestType.Fido2Register,
                "l" => RequestType.Fido2Login,
                _ => RequestType.Default
            };
        }

        private enum RequestType
        {
            Default,
            Fido2Register,
            Fido2Login
        }
    }
}