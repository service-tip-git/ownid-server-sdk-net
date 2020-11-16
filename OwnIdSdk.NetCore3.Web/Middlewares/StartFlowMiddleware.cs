using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using OwnIdSdk.NetCore3.Extensibility.Flow.Contracts;
using OwnIdSdk.NetCore3.Extensibility.Flow.Contracts.Internal;
using OwnIdSdk.NetCore3.Flow.Commands;
using OwnIdSdk.NetCore3.Flow.Commands.Internal;
using OwnIdSdk.NetCore3.Flow.Interfaces;
using OwnIdSdk.NetCore3.Flow.Steps;
using OwnIdSdk.NetCore3.Web.Attributes;

namespace OwnIdSdk.NetCore3.Web.Middlewares
{
    [RequestDescriptor(BaseRequestFields.Context | BaseRequestFields.RequestToken)]
    public class StartFlowMiddleware : BaseMiddleware
    {
        private readonly IFlowRunner _flowRunner;
        private readonly SetWebAppStateCommand _stateCommand;

        public StartFlowMiddleware(RequestDelegate next, IFlowRunner flowRunner, ILogger<StartFlowMiddleware> logger,
            SetWebAppStateCommand stateCommand, StopFlowCommand stopFlowCommand) : base(next, logger, stopFlowCommand)
        {
            _flowRunner = flowRunner;
            _stateCommand = stateCommand;
        }

        protected override async Task Execute(HttpContext httpContext)
        {
            bool.TryParse(httpContext.Request.Query["recovery"], out var requiresRecovery);
            var request = new StateRequest
            {
                EncryptionToken = httpContext.Request.Cookies[_stateCommand.EncryptionCookieName],
                RecoveryToken = httpContext.Request.Cookies[_stateCommand.RecoveryCookieName],
                RequiresRecovery = requiresRecovery
            };

            var stateResult = await _stateCommand.ExecuteAsync(RequestIdentity.Context, request);

            using var requestBodyStreamReader = new StreamReader(httpContext.Request.Body);
            var requestBody = await requestBodyStreamReader.ReadToEndAsync();

            var commandInput = new CommandInput<string>(RequestIdentity, GetRequestCulture(httpContext), requestBody,
                ClientDate);

            var commandResult = await _flowRunner.RunAsync(commandInput, StepType.Starting);

            SetCookies(httpContext.Response, stateResult.Cookies);

            await Json(httpContext, commandResult, StatusCodes.Status200OK);
        }
    }
}