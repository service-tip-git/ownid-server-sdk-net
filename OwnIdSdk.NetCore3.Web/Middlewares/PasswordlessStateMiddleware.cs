using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using OwnIdSdk.NetCore3.Extensibility.Configuration;
using OwnIdSdk.NetCore3.Extensibility.Flow.Contracts;
using OwnIdSdk.NetCore3.Extensibility.Flow.Contracts.Fido2;
using OwnIdSdk.NetCore3.Extensibility.Flow.Contracts.Internal;
using OwnIdSdk.NetCore3.Extensibility.Json;
using OwnIdSdk.NetCore3.Flow.Commands;
using OwnIdSdk.NetCore3.Flow.Commands.Fido2;
using OwnIdSdk.NetCore3.Flow.Commands.Internal;
using OwnIdSdk.NetCore3.Web.Attributes;

namespace OwnIdSdk.NetCore3.Web.Middlewares
{
    [RequestDescriptor(BaseRequestFields.Context)]
    public class PasswordlessStateMiddleware : BaseMiddleware
    {
        private readonly SetPasswordlessStateCommand _stateCommand;
        private readonly IsFido2UserExistsCommand _userExistsCommand;
        private readonly IOwnIdCoreConfiguration _configuration;

        public PasswordlessStateMiddleware(RequestDelegate next, ILogger<PasswordlessStateMiddleware> logger,
            SetPasswordlessStateCommand stateCommand, IsFido2UserExistsCommand userExistsCommand, IOwnIdCoreConfiguration configuration) : base(next, logger)
        {
            _stateCommand = stateCommand;
            _userExistsCommand = userExistsCommand;
            _configuration = configuration;
        }

        protected override async Task Execute(HttpContext httpContext)
        {
            var request = await OwnIdSerializer.DeserializeAsync<InitFido2Request>(httpContext.Request.Body);
            var isFido2Only = _configuration.AuthenticationMode == AuthenticationModeType.Fido2Only;

            var result = new InitFido2Response
            {
                IsFido2Only = isFido2Only
            };

            if (request.IsIncompatible && isFido2Only)
            {
                await Json(httpContext, result, StatusCodes.Status200OK);
                return;
            }

            var stateRequest = new StateRequest
            {
                EncryptionToken = httpContext.Request.Cookies[_stateCommand.EncryptionCookieName],
                RecoveryToken = httpContext.Request.Cookies[_stateCommand.RecoveryCookieName],
                RequiresRecovery = request.IsIncompatible
            };
            
            var stateResult = await _stateCommand.ExecuteAsync(RequestIdentity.Context, stateRequest);
            SetCookies(httpContext.Response, stateResult.Cookies);

            if (!request.IsIncompatible)
            {
                result.Config = new InitFido2Response.ClientSideFido2Config
                {
                    UserName = _configuration.Fido2.UserName,
                    UserDisplayName = _configuration.Fido2.UserDisplayName,
                    RelyingPartyId = _configuration.Fido2.RelyingPartyId,
                    RelyingPartyName = _configuration.Fido2.RelyingPartyName
                };
            }

            if (!string.IsNullOrWhiteSpace(request.CredId))
            {
                var commandInput = new CommandInput<string>(RequestIdentity, GetRequestCulture(httpContext), request.CredId,
                    ClientDate);

                result.UserExists = await _userExistsCommand.ExecuteAsync(commandInput);
            }
                
            
            await Json(httpContext, result, StatusCodes.Status200OK);
        }
    }
}