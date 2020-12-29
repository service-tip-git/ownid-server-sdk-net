using System.Net.Http;
using System.Threading.Tasks;
using OwnID.Commands;
using OwnID.Commands.Fido2;
using OwnID.Extensibility.Cache;
using OwnID.Extensibility.Configuration;
using OwnID.Extensibility.Exceptions;
using OwnID.Extensibility.Flow;
using OwnID.Extensibility.Flow.Contracts;
using OwnID.Extensibility.Flow.Contracts.Jwt;
using OwnID.Extensibility.Flow.Contracts.Start;
using OwnID.Extensibility.Providers;
using OwnID.Flow.Interfaces;
using OwnID.Flow.ResultActions;

namespace OwnID.Flow.TransitionHandlers
{
    public class AcceptStartTransitionHandler : BaseTransitionHandler<TransitionInput<AcceptStartRequest>>
    {
        private readonly IOwnIdCoreConfiguration _coreConfiguration;
        private readonly IIdentitiesProvider _identitiesProvider;
        private readonly VerifyFido2CredentialIdCommand _verifyFido2CredentialIdCommand;
        private readonly SetNewEncryptionTokenCommand _setNewEncryptionTokenCommand;
        private readonly TrySwitchToFido2FlowCommand _trySwitchToFido2FlowCommand;

        public AcceptStartTransitionHandler(IJwtComposer jwtComposer, StopFlowCommand stopFlowCommand,
            IUrlProvider urlProvider, IOwnIdCoreConfiguration coreConfiguration,
            TrySwitchToFido2FlowCommand trySwitchToFido2FlowCommand,
            SetNewEncryptionTokenCommand setNewEncryptionTokenCommand, IIdentitiesProvider identitiesProvider,
            VerifyFido2CredentialIdCommand verifyFido2CredentialIdCommand) : base(StepType.AcceptStart, jwtComposer,
            stopFlowCommand, urlProvider)
        {
            _coreConfiguration = coreConfiguration;
            _trySwitchToFido2FlowCommand = trySwitchToFido2FlowCommand;
            _setNewEncryptionTokenCommand = setNewEncryptionTokenCommand;
            _identitiesProvider = identitiesProvider;
            _verifyFido2CredentialIdCommand = verifyFido2CredentialIdCommand;
        }

        public override FrontendBehavior GetOwnReference(string context, ChallengeType challengeType)
        {
            return new(StepType, challengeType, new CallAction(UrlProvider.GetAcceptStartFlowUrl(context)));
        }

        protected override void Validate(TransitionInput<AcceptStartRequest> input, CacheItem relatedItem)
        {
            if (!relatedItem.IsFido2Flow && string.IsNullOrEmpty(relatedItem.EncToken)
                                         && string.IsNullOrEmpty(relatedItem.Fido2CredentialId)
                                         && relatedItem.ChallengeType == ChallengeType.Login
                                         && (!_coreConfiguration.TFAEnabled || !input.Data.SupportsFido2))
                throw new OwnIdException(ErrorType.UserNotFound);
        }

        protected override async Task<ITransitionResult> ExecuteInternalAsync(TransitionInput<AcceptStartRequest> input,
            CacheItem relatedItem)
        {
            var composeInfo = new BaseJwtComposeInfo
            {
                Context = input.Context,
                ClientTime = input.ClientDate,
                Locale = input.CultureInfo?.Name,
                IncludeFido2FallbackBehavior = true
            };

            if (_coreConfiguration.TFAEnabled)
            {
                if (input.Data.SupportsFido2)
                {
                    // check if fido2 page response available
                    if (string.IsNullOrWhiteSpace(input.Data.ExtAuthPayload))
                    {
                        await _verifyFido2CredentialIdCommand.ExecuteAsync(relatedItem);
                        var fido2Url = UrlProvider.GetFido2Url(relatedItem.Context, relatedItem.RequestToken,input.CultureInfo?.Name);
                        composeInfo.Behavior = FrontendBehavior.CreateRedirect(fido2Url);
                    }
                    else
                    {
                        var switchResult =
                            await _trySwitchToFido2FlowCommand.ExecuteAsync(relatedItem, input.Data.ExtAuthPayload);
                        
                        if(switchResult == null && _coreConfiguration.Fido2FallbackBehavior == Fido2FallbackBehavior.Block)
                            return CreateErrorResponse(input, ErrorType.RequiresBiometricInput);
                        
                        composeInfo.Behavior = switchResult ?? GetNextBehaviorFunc(input, relatedItem);
                    }

                    return new JwtContainer(JwtComposer.GenerateBaseStepJwt(composeInfo));
                }

                // if FIDO2 flow or there is no way to login (with/without recovery with previously created creds)
                if (relatedItem.IsFido2Flow
                    || _coreConfiguration.Fido2FallbackBehavior == Fido2FallbackBehavior.Block
                    && relatedItem.ChallengeType != ChallengeType.Login && (!input.Data.AuthType.HasValue
                                                                            || !string.IsNullOrEmpty(relatedItem
                                                                                .RecoveryToken)))
                    return CreateErrorResponse(input, ErrorType.RequiresBiometricInput);

                // go to passcode if such behavior enabled and check if create
                if (_coreConfiguration.Fido2FallbackBehavior == Fido2FallbackBehavior.Passcode
                    && (relatedItem.EncTokenEnding == CookieValuesConstants.PasscodeEnding
                        || relatedItem.ChallengeType != ChallengeType.Login))
                    composeInfo.Behavior = new FrontendBehavior(StepType.EnterPasscode, relatedItem.ChallengeType,
                        GetNextBehaviorFunc(input, relatedItem))
                    {
                        AlternativeBehavior = new FrontendBehavior(StepType.ResetPasscode, relatedItem.ChallengeType,
                            new CallAction(UrlProvider.GetResetPasscodeUrl(relatedItem.Context),
                                HttpMethod.Delete.ToString()))
                    };
            }

            if (!string.IsNullOrWhiteSpace(relatedItem.EncToken))
            {
                composeInfo.EncToken = relatedItem.EncToken;
            }
            else
            {
                var updatedItem = await _setNewEncryptionTokenCommand.ExecuteAsync(relatedItem.Context);
                composeInfo.EncToken = updatedItem.EncToken;
                // TODO: rework
                relatedItem = updatedItem;
            }

            composeInfo.CanBeRecovered = !string.IsNullOrEmpty(relatedItem.RecoveryToken);

            if (!_coreConfiguration.TFAEnabled
                || _coreConfiguration.Fido2FallbackBehavior == Fido2FallbackBehavior.Basic)
                composeInfo.Behavior = GetNextBehaviorFunc(input, relatedItem);

            return new JwtContainer(JwtComposer.GenerateBaseStepJwt(composeInfo,
                relatedItem.DID ?? _identitiesProvider.GenerateUserId()));
        }
    }
}