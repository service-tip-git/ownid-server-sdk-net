using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using OwnIdSdk.NetCore3.Extensibility.Flow.Contracts;
using OwnIdSdk.NetCore3.Extensibility.Flow.Contracts.ConnectionRecovery;
using OwnIdSdk.NetCore3.Extensibility.Json;
using OwnIdSdk.NetCore3.Flow.Commands.ConnectionRecovery;
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
            var request = await OwnIdSerializer.DeserializeAsync<SetPasswordlessStateRequest>(httpContext.Request.Body);
            var result = await _stateCommand.ExecuteAsync(RequestIdentity.Context, request);
            await Json(httpContext, result, StatusCodes.Status200OK);
        }
    }
}