using System.Threading.Tasks;
using OwnID.Extensibility.Exceptions;
using OwnID.Flow.Interfaces;
using OwnID.Flow.Steps;
using OwnID.Services;
using OwnID.Extensibility.Flow;
using OwnID.Extensibility.Flow.Contracts.Jwt;

namespace OwnID.Flow.Commands
{
    public class StopFlowCommand
    {
        private readonly ICacheItemService _cacheItemService;
        private readonly IJwtComposer _jwtComposer;

        public StopFlowCommand(ICacheItemService cacheItemService, IJwtComposer jwtComposer)
        {
            _cacheItemService = cacheItemService;
            _jwtComposer = jwtComposer;
        }

        public async Task<ICommandResult> ExecuteAsync(ICommandInput input, ErrorType errorType, string errorMessage)
        {
            await _cacheItemService.FinishFlowWithErrorAsync(input.Context, errorMessage);

            var composeInfo = new BaseJwtComposeInfo
            {
                Context = input.Context,
                ClientTime = input.ClientDate,
                Locale = input.CultureInfo?.Name,
                Behavior = FrontendBehavior.CreateError(errorType)
            };

            var jwt = _jwtComposer.GenerateFinalStepJwt(composeInfo);
            return new JwtContainer(jwt);
        }
    }
}