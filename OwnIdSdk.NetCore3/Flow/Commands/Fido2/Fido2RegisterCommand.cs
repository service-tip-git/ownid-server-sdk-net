using System.Text;
using System.Threading.Tasks;
using Fido2NetLib;
using Fido2NetLib.Objects;
using OwnIdSdk.NetCore3.Extensibility.Cache;
using OwnIdSdk.NetCore3.Extensibility.Configuration;
using OwnIdSdk.NetCore3.Extensibility.Exceptions;
using OwnIdSdk.NetCore3.Extensibility.Flow;
using OwnIdSdk.NetCore3.Extensibility.Flow.Abstractions;
using OwnIdSdk.NetCore3.Extensibility.Flow.Contracts.Fido2;
using OwnIdSdk.NetCore3.Extensibility.Flow.Contracts.Jwt;
using OwnIdSdk.NetCore3.Flow.Interfaces;
using OwnIdSdk.NetCore3.Flow.Steps;
using OwnIdSdk.NetCore3.Services;

namespace OwnIdSdk.NetCore3.Flow.Commands.Fido2
{
    public class Fido2RegisterCommand : BaseFlowCommand
    {
        private readonly IFido2 _fido2;
        private readonly ICacheItemService _cacheItemService;
        private readonly IJwtComposer _jwtComposer;
        private readonly IFlowController _flowController;
        private readonly IOwnIdCoreConfiguration _configuration;
        private readonly IAccountLinkHandler _linkHandler;
        private readonly IAccountRecoveryHandler _recoveryHandler;

        public Fido2RegisterCommand(
            IFido2 fido2,
            ICacheItemService cacheItemService,
            IJwtComposer jwtComposer,
            IFlowController flowController,
            IOwnIdCoreConfiguration configuration,
            IAccountLinkHandler linkHandler,
            IAccountRecoveryHandler recoveryHandler)
        {
            _fido2 = fido2;
            _cacheItemService = cacheItemService;
            _jwtComposer = jwtComposer;
            _flowController = flowController;
            _configuration = configuration;
            _linkHandler = linkHandler;
            _recoveryHandler = recoveryHandler;
        }

        protected override void Validate(ICommandInput input, CacheItem relatedItem)
        {
            //throw new System.NotImplementedException();
        }

        protected override async Task<ICommandResult> ExecuteInternal(ICommandInput input, CacheItem relatedItem,
            StepType currentStepType)
        {
            if (!(input is CommandInput<Fido2RegisterRequest> request))
                throw new InternalLogicException($"Incorrect input type for {nameof(Fido2RegisterCommand)}");

            var fido2Response = new AuthenticatorAttestationRawResponse
            {
                Id = Base64Url.Decode(request.Data.Info.UserId),
                RawId = Base64Url.Decode(request.Data.Info.UserId),
                Type = PublicKeyCredentialType.PublicKey,
                Response = new AuthenticatorAttestationRawResponse.ResponseData
                {
                    AttestationObject = Base64Url.Decode(request.Data.Info.AttestationObject),
                    ClientDataJson = Base64Url.Decode(request.Data.Info.ClientDataJSON)
                }
            };

            var options = new CredentialCreateOptions
            {
                Challenge = Encoding.ASCII.GetBytes(relatedItem.Context),
                Rp = new PublicKeyCredentialRpEntity(
                    _configuration.Fido2.RelyingPartyId,
                    _configuration.Fido2.RelyingPartyName,
                    null),
                User = new Fido2User
                {
                    DisplayName = _configuration.Fido2.UserDisplayName,
                    Name = _configuration.Fido2.UserName,
                    Id = Base64Url.Decode(request.Data.Info.UserId)
                }
            };

            var result = await _fido2.MakeNewCredentialAsync(
                fido2Response,
                options,
                async args => await Task.FromResult(true));

            var publicKey = Base64Url.Encode(result.Result.PublicKey);

            switch (relatedItem.FlowType)
            {
                case FlowType.PartialAuthorize:
                    await _cacheItemService.SetPublicKeyAsync(
                        relatedItem.Context,
                        publicKey,
                        result.Result.Counter,
                        request.Data.Info.UserId,
                        Base64Url.Encode(result.Result.CredentialId)
                    );

                    await _cacheItemService.FinishAuthFlowSessionAsync(relatedItem.Context, request.Data.Info.UserId);
                    break;
                case FlowType.Link:
                case FlowType.LinkWithPin:
                    await _linkHandler.OnLinkAsync(
                        relatedItem.DID,
                        publicKey,
                        request.Data.Info.UserId,
                        Base64Url.Encode(result.Result.CredentialId),
                        result.Result.Counter
                    );

                    await _cacheItemService.FinishAuthFlowSessionAsync(relatedItem.Context, relatedItem.DID);
                    break;
                case FlowType.Recover:
                case FlowType.RecoverWithPin:
                    var recoverResult = await _recoveryHandler.RecoverAsync(relatedItem.Payload);
                    await _recoveryHandler.OnRecoverAsync(
                        recoverResult.DID,
                        publicKey,
                        request.Data.Info.UserId,
                        Base64Url.Encode(result.Result.CredentialId),
                        result.Result.Counter);

                    await _cacheItemService.FinishAuthFlowSessionAsync(relatedItem.Context, relatedItem.DID);
                    break;
            }

            var jwt = _jwtComposer.GenerateBaseStep(
                relatedItem.Context,
                _flowController.GetExpectedFrontendBehavior(relatedItem, currentStepType),
                request.Data.Info.UserId,
                input.CultureInfo?.Name,
                true);

            return new JwtContainer(jwt);
        }
    }
}