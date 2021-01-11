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
    public class UpgradeAuthTypeMiddleware : BaseMiddleware
    {
        public const string NewAuthTypeRouteName = "authType";
        private readonly IFlowRunner _flowRunner;

        public UpgradeAuthTypeMiddleware(RequestDelegate next, ILogger<CheckUserExistenceMiddleware> logger,
            IFlowRunner flowRunner) : base(next, logger)
        {
            _flowRunner = flowRunner;
        }

        protected override async Task ExecuteAsync(HttpContext httpContext)
        {
            // Get new auth type from route
            if (!Enum.TryParse(httpContext.GetRouteValue(NewAuthTypeRouteName).ToString(),
                true, out ConnectionAuthType newAuthType))
            {
                httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
                return;
            }
            
            switch (newAuthType)
            {
                case ConnectionAuthType.Passcode:
                {
                    var jwtContainer = await GetRequestJwtContainerAsync(httpContext);

                    var commandInput = new TransitionInput<JwtContainer>(RequestIdentity,
                        GetRequestCulture(httpContext),
                        jwtContainer, ClientDate);

                    var result = await _flowRunner.RunAsync(commandInput, StepType.UpgradeToPasscode);
                    await JsonAsync(httpContext, result, StatusCodes.Status200OK);
                    break;
                }
                case ConnectionAuthType.Fido2:
                {
                    using var bodyReader = new StreamReader(httpContext.Request.Body);
                    var bodyStr = await bodyReader.ReadToEndAsync();

                    var fidoResult = await _flowRunner.RunAsync(
                        new TransitionInput<string>(RequestIdentity, GetRequestCulture(httpContext), bodyStr,
                            ClientDate),
                        StepType.UpgradeToFido2);

                    SetCookies(httpContext.Response, fidoResult);
                    await JsonAsync(httpContext, fidoResult, StatusCodes.Status200OK);
                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}