using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using OwnIdSdk.NetCore3.Extensibility.Flow.Contracts;
using OwnIdSdk.NetCore3.Extensibility.Flow.Contracts.Internal;
using OwnIdSdk.NetCore3.Flow.Commands.Internal;
using OwnIdSdk.NetCore3.Web.Attributes;

namespace OwnIdSdk.NetCore3.Web.Middlewares
{
    [RequestDescriptor(BaseRequestFields.Context)]
    public class PasswordlessStateMiddleware : BaseMiddleware
    {
        private readonly SetPasswordlessStateCommand _stateCommand;

        public PasswordlessStateMiddleware(RequestDelegate next, ILogger<PasswordlessStateMiddleware> logger,
            SetPasswordlessStateCommand stateCommand) : base(next, logger)
        {
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

            var result = await _stateCommand.ExecuteAsync(RequestIdentity.Context, request);

            SetCookies(httpContext.Response, result.Cookies);
            OkNoContent(httpContext.Response);
        }
    }
}