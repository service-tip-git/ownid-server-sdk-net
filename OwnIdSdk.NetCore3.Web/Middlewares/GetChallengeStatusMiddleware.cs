using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using OwnIdSdk.NetCore3.Configuration;
using OwnIdSdk.NetCore3.Contracts;
using OwnIdSdk.NetCore3.Store;
using OwnIdSdk.NetCore3.Web.Abstractions;

namespace OwnIdSdk.NetCore3.Web.Middlewares
{
    public class GetChallengeStatusMiddleware : BaseMiddleware
    {
        public GetChallengeStatusMiddleware(RequestDelegate next, IChallengeHandler challengeHandler,
            ICacheStore cacheStore, ProviderConfiguration providerConfiguration) : base(next, challengeHandler,
            cacheStore, providerConfiguration)
        {
        }

        public override async Task InvokeAsync(HttpContext context)
        {
            var routeData = context.GetRouteData();
            var challengeContext = routeData.Values["context"]?.ToString();

            if (string.IsNullOrEmpty(challengeContext) || !Provider.IsContextFormatValid(challengeContext))
            {
                NotFound(context.Response);
                return;
            }

            var request = await JsonSerializer.DeserializeAsync<GetStatusRequest>(context.Request.Body);

            if (string.IsNullOrEmpty(request.Nonce))
            {
                NotFound(context.Response);
                return;
            }

            var didResult = await Provider.GetDIDAsync(challengeContext, request.Nonce);

            if (didResult.isSuccess)
            {
                await Provider.RemoveContextAsync(challengeContext);
                
                var result = await ChallengeHandler.OnSuccessLoginAsync(didResult.did, context.Response);
                
                await Json(context.Response, result.Data, result.HttpCode);
                return;
            }
            
            await Ok(context.Response, new GetStatusResponse{IsSuccess = didResult.isSuccess});
        }
    }
}