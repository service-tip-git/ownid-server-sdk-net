using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using OwnIdSdk.NetCore3.Extensibility.Flow.Contracts;
using OwnIdSdk.NetCore3.Flow;
using OwnIdSdk.NetCore3.Flow.Commands;
using OwnIdSdk.NetCore3.Flow.Interfaces;
using OwnIdSdk.NetCore3.Web.Attributes;

namespace OwnIdSdk.NetCore3.Web.Middlewares.Link
{
    [Obsolete]
    [RequestDescriptor(BaseRequestFields.Context | BaseRequestFields.RequestToken | BaseRequestFields.ResponseToken)]
    public class GetAccountLinkDataMiddleware : BaseMiddleware
    {
        private readonly IFlowRunner _flowRunner;

        public GetAccountLinkDataMiddleware(RequestDelegate next, IFlowRunner flowRunner,
            ILogger<GetAccountLinkDataMiddleware> logger) : base(next, logger)
        {
            _flowRunner = flowRunner;
        }

        protected override async Task Execute(HttpContext httpContext)
        {
            var result = await _flowRunner.RunAsync(new CommandInput
            {
                Context = RequestIdentity.Context,
                RequestToken = RequestIdentity.RequestToken,
                ResponseToken = RequestIdentity.RequestToken,
                CultureInfo = GetRequestCulture(httpContext)
            }, StepType.ApprovePin);
            
             // result = await _getLinkProfileCommand.ExecuteAsync(RequestIdentity.Context, StepType.Link, false,
             //    GetRequestCulture(httpContext));

            await Json(httpContext, result, StatusCodes.Status200OK);
        }
    }
}