using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using OwnIdSdk.NetCore3.Flow.Commands;
using OwnIdSdk.NetCore3.Flow.Commands.Fido2;

namespace OwnIdSdk.NetCore3.Web.Middlewares
{
    public class IsFido2UserExistsMiddleware : BaseMiddleware
    {
        public const string CredentialIdRouteName = "credentialId";

        private readonly IsFido2UserExistsCommand _isFido2UserExistsCommand;

        public IsFido2UserExistsMiddleware(RequestDelegate next, ILogger<IsFido2UserExistsMiddleware> logger,
            IsFido2UserExistsCommand isFido2UserExistsCommand) : base(next, logger)
        {
            _isFido2UserExistsCommand = isFido2UserExistsCommand;
        }

        protected override async Task Execute(HttpContext httpContext)
        {
            var credentialId = httpContext.GetRouteData().Values[CredentialIdRouteName]?.ToString();
            if (string.IsNullOrEmpty(credentialId))
            {
                httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
                return;
            }

            var commandInput = new CommandInput<string>(RequestIdentity, GetRequestCulture(httpContext), credentialId,
                ClientDate);

            var commandResult = await _isFido2UserExistsCommand.ExecuteAsync(commandInput);

            await Json(httpContext, new {isUserExists = commandResult}, StatusCodes.Status200OK);
        }
    }
}