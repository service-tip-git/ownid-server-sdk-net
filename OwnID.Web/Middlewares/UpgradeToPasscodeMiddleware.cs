using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using OwnID.Extensibility.Flow.Contracts;
using OwnID.Extensibility.Flow.Contracts.Jwt;
using OwnID.Extensibility.Flow.Contracts.Start;
using OwnID.Flow;
using OwnID.Flow.Interfaces;
using OwnID.Web.Attributes;

namespace OwnID.Web.Middlewares
{
    [RequestDescriptor(BaseRequestFields.Context | BaseRequestFields.RequestToken | BaseRequestFields.ResponseToken)]
    public class UpgradeToPasscodeMiddleware : BaseMiddleware
    {
        private readonly IFlowRunner _flowRunner;

        public UpgradeToPasscodeMiddleware(RequestDelegate next, ILogger<CheckUserExistenceMiddleware> logger,
            IFlowRunner flowRunner) : base(next, logger)
        {
            _flowRunner = flowRunner;
        }

        protected override async Task ExecuteAsync(HttpContext httpContext)
        {
            var jwtContainer = await GetRequestJwtContainerAsync(httpContext);

            var commandInput = new TransitionInput<JwtContainer>(RequestIdentity,
                GetRequestCulture(httpContext),
                jwtContainer, ClientDate);

            var result = await _flowRunner.RunAsync(commandInput, StepType.UpgradeToPasscode);
            await JsonAsync(httpContext, result, StatusCodes.Status200OK);
        }
    }
}