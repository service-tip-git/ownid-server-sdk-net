using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using OwnID.Extensibility.Configuration;
using OwnID.Extensibility.Flow.Contracts;
using OwnID.Extensibility.Flow.Contracts.Fido2;
using OwnID.Extensibility.Flow.Contracts.Internal;
using OwnID.Extensibility.Json;
using OwnID.Flow.Commands;
using OwnID.Flow.Commands.Internal;
using OwnID.Web.Attributes;

namespace OwnID.Web.Middlewares
{
    [RequestDescriptor(BaseRequestFields.Context)]
    public class PasswordlessStateMiddleware : BaseMiddleware
    {
        private readonly IOwnIdCoreConfiguration _configuration;
        private readonly SetPasswordlessStateCommand _stateCommand;
        private readonly CheckUserExistenceCommand _userExistenceCommand;

        public PasswordlessStateMiddleware(RequestDelegate next, ILogger<PasswordlessStateMiddleware> logger,
            SetPasswordlessStateCommand stateCommand, CheckUserExistenceCommand userExistenceCommand,
            IOwnIdCoreConfiguration configuration, StopFlowCommand stopFlowCommand) : base(next, logger,
            stopFlowCommand)
        {
            _stateCommand = stateCommand;
            _userExistenceCommand = userExistenceCommand;
            _configuration = configuration;
        }

        protected override async Task Execute(HttpContext httpContext)
        {
            var request = await OwnIdSerializer.DeserializeAsync<InitFido2Request>(httpContext.Request.Body);
            var isFido2Only = _configuration.AuthenticationMode == AuthenticationModeType.Fido2Only;
            var isRegistration = request.FlowType == "r";

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
                RequiresRecovery = request.IsIncompatible,
                CredId = request.CredId
            };

            var stateResult = await _stateCommand.ExecuteAsync(RequestIdentity.Context, stateRequest);
            SetCookies(httpContext.Response, stateResult.Cookies);

            if (!request.IsIncompatible)
                result.Config = new InitFido2Response.ClientSideFido2Config
                {
                    UserName = _configuration.Fido2.UserName,
                    UserDisplayName = _configuration.Fido2.UserDisplayName,
                    RelyingPartyId = _configuration.Fido2.RelyingPartyId,
                    RelyingPartyName = _configuration.Fido2.RelyingPartyName
                };

            var credIdToCheck = request.CredId ?? httpContext.Request.Cookies[_stateCommand.CredIdCookieName];
            
            if (!string.IsNullOrWhiteSpace(credIdToCheck))
            {
                // Check if user exists
                // If yes during registration - cancel registration -> 
                // if no during login - switch to "link at login" flow (at WebApp)
                var existenceRequest = new UserExistsRequest
                {
                    AuthenticatorType = ExtAuthenticatorType.Fido2,
                    ErrorOnExisting = isRegistration,
                    UserIdentifier = credIdToCheck
                };

                var commandInput = new CommandInput<UserExistsRequest>(RequestIdentity, GetRequestCulture(httpContext),
                    existenceRequest, ClientDate);

                result.UserExists = await _userExistenceCommand.Check(commandInput);
                result.CredId = credIdToCheck;
            }

            await Json(httpContext, result, StatusCodes.Status200OK);
        }
    }
}