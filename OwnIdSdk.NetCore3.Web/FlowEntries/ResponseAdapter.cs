using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using OwnIdSdk.NetCore3.Contracts;
using OwnIdSdk.NetCore3.Contracts.Jwt;
using OwnIdSdk.NetCore3.Web.Abstractions;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace OwnIdSdk.NetCore3.Web.FlowEntries
{
    public class ChallengeHandlerAdapter<T> : IChallengeHandlerAdapter where T : class
    {
        private readonly IChallengeHandler<T> _adaptee;
        
        public ChallengeHandlerAdapter(IChallengeHandler<T> adaptee)
        {
            _adaptee = adaptee;
        }

        public IUserProfileContext CreateUserDefinedContext(UserProfile profile, 
            ILocalizationService localizationService)
        {
            return new UserProfileFormContext<T>(profile.DID, profile.PublicKey,
                JsonSerializer.Deserialize<T>(profile.Profile.GetRawText()), localizationService);
        }

        public async Task UpdateProfileAsync(IUserProfileContext context)
        {
            await _adaptee.UpdateProfileAsync(context as UserProfileFormContext<T>);
        }

        public async Task<LoginResult<object>> OnSuccessLoginAsync(string did, HttpResponse response)
        {
            return await _adaptee.OnSuccessLoginAsync(did, response);
        }
    }
}