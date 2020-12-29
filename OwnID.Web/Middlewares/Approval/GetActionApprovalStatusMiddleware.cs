using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using OwnID.Commands;
using OwnID.Extensibility.Flow.Contracts;
using OwnID.Flow;
using OwnID.Flow.Interfaces;
using OwnID.Web.Attributes;

namespace OwnID.Web.Middlewares.Approval
{
    [RequestDescriptor(BaseRequestFields.Context | BaseRequestFields.RequestToken | BaseRequestFields.ResponseToken)]
    public class GetActionApprovalStatusMiddleware : BaseMiddleware
    {
        private readonly IFlowRunner _flowRunner;

        public GetActionApprovalStatusMiddleware(RequestDelegate next, IFlowRunner flowRunner,
            ILogger<GetActionApprovalStatusMiddleware> logger) : base(next, logger)
        {
            _flowRunner = flowRunner;
        }

        protected override async Task ExecuteAsync(HttpContext httpContext)
        {
            var result = await _flowRunner.RunAsync(
                new TransitionInput(RequestIdentity, GetRequestCulture(httpContext), ClientDate),
                StepType.ApprovePin);

            await JsonAsync(httpContext, result, StatusCodes.Status200OK);
        }
    }
}