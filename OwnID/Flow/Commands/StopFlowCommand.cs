using System.Threading.Tasks;
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

        public async Task<ICommandResult> ExecuteAsync(ICommandInput input, string errorMessage)
        {
            await _cacheItemService.FinishFlowWithErrorAsync(input.Context, errorMessage);

            var relatedItem = await _cacheItemService.GetCacheItemByContextAsync(input.Context);

            var composeInfo = new BaseJwtComposeInfo
            {
                Context = input.Context,
                ClientTime = input.ClientDate,
                Locale = input.CultureInfo?.Name,
                Behavior = new FrontendBehavior
                {
                    Type = StepType.Error,
                    ActionType = ActionType.Finish,
                    //
                    // TODO: Remove getting challenge type from cache when we change error handling model 
                    //
                    ChallengeType = relatedItem.ChallengeType
                }
            };

            var jwt = _jwtComposer.GenerateFinalStepJwt(composeInfo);
            return new JwtContainer(jwt);
        }
    }
}