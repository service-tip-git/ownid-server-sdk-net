using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using OwnID.Cryptography;
using OwnID.Flow.Adapters;
using OwnID.Services;
using OwnID.Extensibility.Cache;
using OwnID.Extensibility.Flow;
using OwnID.Extensibility.Flow.Contracts;
using OwnID.Extensibility.Services;

namespace OwnID.Flow.Commands
{
    public class GetStatusCommand
    {
        private readonly ICacheItemService _cacheItemService;
        private readonly ILocalizationService _localizationService;
        private readonly IJwtService _jwtService;
        private readonly IUserHandlerAdapter _userHandlerAdapter;

        public GetStatusCommand(IUserHandlerAdapter userHandlerAdapter, ICacheItemService cacheItemService,
            ILocalizationService localizationService, IJwtService jwtService)
        {
            _userHandlerAdapter = userHandlerAdapter;
            _cacheItemService = cacheItemService;
            _localizationService = localizationService;
            _jwtService = jwtService;
        }

        public async Task<List<GetStatusResponse>> ExecuteAsync(List<GetStatusRequest> request)
        {
            var response = new List<GetStatusResponse>();

            foreach (var statusRequestItem in request)
            {
                var responseItem = await GetContextStatus(statusRequestItem);

                if (responseItem == null)
                    continue;

                if (responseItem.Status == CacheItemStatus.Finished)
                {
                    response = new List<GetStatusResponse>(1) {responseItem};
                    break;
                }

                response.Add(responseItem);
            }

            return response;
        }

        private async Task<GetStatusResponse> GetContextStatus(GetStatusRequest requestItem)
        {
            if (string.IsNullOrEmpty(requestItem.Context) || string.IsNullOrEmpty(requestItem.Nonce))
                return null;

            var cacheItem =
                await _cacheItemService.PopFinishedCacheItemAsync(requestItem.Context, requestItem.Nonce);

            if (cacheItem == null)
                return null;

            var result = new GetStatusResponse
            {
                Status = cacheItem.Status,
                Context = requestItem.Context
            };

            if (cacheItem.SecurityCode != null && cacheItem.Status == CacheItemStatus.WaitingForApproval)
                // TODO: refactor to entity
                result.Payload = new
                {
                    data = new
                    {
                        pin = cacheItem.SecurityCode
                    }
                };

            if (cacheItem.Status != CacheItemStatus.Finished)
                return result;

            if (!string.IsNullOrEmpty(cacheItem.Error))
            {
                result.Payload = new AuthResult<object>(cacheItem.Error);
                return result;
            }

            if (cacheItem.FlowType == FlowType.Authorize)
            {
                result.Payload = await _userHandlerAdapter.OnSuccessLoginAsync(cacheItem.DID, cacheItem.PublicKey);
            }
            else
            {
                switch (cacheItem.ChallengeType)
                {
                    case ChallengeType.Login
                        when cacheItem.FlowType == FlowType.Fido2Login
                             && string.IsNullOrWhiteSpace(cacheItem.Fido2CredentialId):
                    {
                        var errorMessage = _localizationService.GetLocalizedString("Error_UserNotFound");
                        result.Payload = new AuthResult<object>(errorMessage);
                        break;
                    }
                    case ChallengeType.Login
                        when !string.IsNullOrWhiteSpace(cacheItem.Fido2CredentialId)
                             && cacheItem.Fido2SignatureCounter.HasValue:
                    {
                        result.Payload = await _userHandlerAdapter.OnSuccessLoginByFido2Async(
                            cacheItem.Fido2CredentialId,
                            cacheItem.Fido2SignatureCounter.Value);
                        break;
                    }
                    case ChallengeType.Login
                        when await _userHandlerAdapter.IsUserExistsAsync(cacheItem.PublicKey):
                    {
                        result.Payload = await _userHandlerAdapter.OnSuccessLoginByPublicKeyAsync(cacheItem.PublicKey);
                        break;
                    }
                    case ChallengeType.Login:
                        result.Payload = SetPartialRegisterResult(cacheItem);
                        break;
                    case ChallengeType.Register
                        when await _userHandlerAdapter.IsUserExistsAsync(cacheItem.PublicKey):
                    {
                        var errorMessage = _localizationService.GetLocalizedString("Error_PhoneAlreadyConnected");
                        result.Payload = new AuthResult<object>(errorMessage);
                        return result;
                    }
                    case ChallengeType.Register
                        when string.IsNullOrWhiteSpace(cacheItem.Fido2CredentialId):
                    {
                        result.Payload = SetPartialRegisterResult(cacheItem);
                        break;
                    }
                    case ChallengeType.Register:
                        result.Payload = new
                        {
                            data = new
                            {
                                pubKey = cacheItem.PublicKey,
                                fido2SignatureCounter = cacheItem.Fido2SignatureCounter.ToString(),
                                fido2CredentialId = cacheItem.Fido2CredentialId
                            }
                        };
                        break;
                    //
                    // TODO: fix needed at web-ui-sdk to avoid error in console if data is undefined
                    //
                    case ChallengeType.Recover:
                    case ChallengeType.Link:
                        result.Payload = new
                        {
                            data = new { }
                        };
                        break;
                }
            }

            result.Metadata = _jwtService.GenerateDataJwt(new Dictionary<string, object>
            {
                {
                    "data", new
                    {
                        action = cacheItem.ChallengeType.ToString(),
                        authType = cacheItem.GetAuthType()
                    }
                }
            });

            return result;
        }

        private object SetPartialRegisterResult(CacheItem cacheItem)
        {
            using var sha256 = new SHA256Managed();
            var hash = Convert.ToBase64String(
                sha256.ComputeHash(Encoding.UTF8.GetBytes(cacheItem.PublicKey)));

            return new
            {
                data = new
                {
                    pubKey = cacheItem.PublicKey,
                    keyHsh = hash,
                    recoveryId = cacheItem.RecoveryToken,
                    recoveryEncData = cacheItem.RecoveryData
                }
            };
        }
    }
}