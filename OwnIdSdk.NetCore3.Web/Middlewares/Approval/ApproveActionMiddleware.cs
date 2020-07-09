using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using OwnIdSdk.NetCore3.Configuration;
using OwnIdSdk.NetCore3.Contracts;
using OwnIdSdk.NetCore3.Store;
using OwnIdSdk.NetCore3.Web.Exceptions;
using OwnIdSdk.NetCore3.Web.FlowEntries.RequestHandling;

namespace OwnIdSdk.NetCore3.Web.Middlewares.Approval
{
    [RequestDescriptor(BaseRequestFields.Context)]
    public class ApproveActionMiddleware : BaseMiddleware
    {
        public ApproveActionMiddleware(RequestDelegate next, IOwnIdCoreConfiguration coreConfiguration,
            ICacheStore cacheStore, ILocalizationService localizationService,
            ILogger<ApproveActionMiddleware> logger) : base(next,
            coreConfiguration, cacheStore, localizationService, logger)
        {
        }

        protected override async Task Execute(HttpContext httpContext)
        {
            var request = await JsonSerializer.DeserializeAsync<ApproveActionRequest>(httpContext.Request.Body);

            if (string.IsNullOrEmpty(request.Context) || string.IsNullOrEmpty(request.Nonce))
                throw new RequestValidationException("Context and nonce are required");

            await OwnIdProvider.SetApprovalResolutionAsync(request.Context, request.Nonce, request.IsApproved);
            OkNoContent(httpContext.Response);
        }
    }
}