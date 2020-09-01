using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using OwnIdSdk.NetCore3.Extensibility.Cache;
using OwnIdSdk.NetCore3.Extensibility.Flow;
using OwnIdSdk.NetCore3.Extensibility.Flow.Contracts;
using OwnIdSdk.NetCore3.Flow.Adapters;
using OwnIdSdk.NetCore3.Services;

namespace OwnIdSdk.NetCore3.Flow.Commands
{
    public class GetStatusCommand
    {
        private readonly ICacheItemService _cacheItemService;
        private readonly IUserHandlerAdapter _userHandlerAdapter;

        public GetStatusCommand(IUserHandlerAdapter userHandlerAdapter, ICacheItemService cacheItemService)
        {
            _userHandlerAdapter = userHandlerAdapter;
            _cacheItemService = cacheItemService;
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
                await _cacheItemService.PopFinishedAuthFlowSessionAsync(requestItem.Context, requestItem.Nonce);

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

            if (cacheItem.FlowType == FlowType.Authorize)
            {
                result.Payload = await _userHandlerAdapter.OnSuccessLoginAsync(cacheItem.DID, cacheItem.PublicKey);
            }
            else
            {
                if (cacheItem.ChallengeType == ChallengeType.Login)
                {
                    if (!string.IsNullOrWhiteSpace(cacheItem.Fido2CredentialId) && cacheItem.Fido2SignatureCounter.HasValue)
                    {
                        result.Payload = await _userHandlerAdapter.OnSuccessLoginByFido2Async(cacheItem.Fido2CredentialId,
                            cacheItem.Fido2SignatureCounter.Value);
                    }
                    else
                    {
                        result.Payload = await _userHandlerAdapter.OnSuccessLoginByPublicKeyAsync(cacheItem.PublicKey);
                    }
                }
                else if (cacheItem.ChallengeType == ChallengeType.Register)
                {
                    if (await _userHandlerAdapter.IsUserExists(cacheItem.PublicKey))
                    {
                        result.Payload = new LoginResult<object>("User associated with this mobile device already exist");
                        return result;
                    }
                    
                    if (string.IsNullOrWhiteSpace(cacheItem.Fido2CredentialId))
                    {
                        using var sha256 = new SHA256Managed();
                        var hash = Convert.ToBase64String(
                            sha256.ComputeHash(Encoding.UTF8.GetBytes(cacheItem.PublicKey)));

                        result.Payload = new
                        {
                            data = new
                            {
                                pubKey = cacheItem.PublicKey,
                                keyHsh = hash,
                            }
                        };
                    }
                    else
                    {
                        result.Payload = new
                        {
                            data = new
                            {
                                pubKey = cacheItem.PublicKey,
                                fido2SignatureCounter = cacheItem.Fido2SignatureCounter,
                                fido2CredentialId = cacheItem.Fido2CredentialId
                            }
                        };
                    }
                }
                //
                // TODO: fix needed at web-ui-sdk to avoid error in console if data is undefined
                //
                else if (cacheItem.ChallengeType == ChallengeType.Recover
                         || cacheItem.ChallengeType == ChallengeType.Link)
                {
                    result.Payload = new
                    {
                        data = new { }
                    };
                }
            }

            return result;
        }
    }
}