using OwnID.Extensibility.Exceptions;
using OwnID.Extensibility.Flow.Contracts.Jwt;
using OwnID.Flow;
using OwnID.Flow.Interfaces;
using OwnID.Flow.ResultActions;

namespace OwnID.Commands
{
    public class ErrorFlowResponseCommand
    {
        private readonly IJwtComposer _jwtComposer;

        public ErrorFlowResponseCommand(IJwtComposer jwtComposer)
        {
            _jwtComposer = jwtComposer;
        }

        public JwtContainer Execute(ITransitionInput input, ErrorType errorType)
        {
            var composeInfo = new BaseJwtComposeInfo(input)
            {
                Behavior = FrontendBehavior.CreateError(errorType)
            };

            var jwt = _jwtComposer.GenerateFinalStepJwt(composeInfo);
            return new JwtContainer(jwt);
        }
    }
}