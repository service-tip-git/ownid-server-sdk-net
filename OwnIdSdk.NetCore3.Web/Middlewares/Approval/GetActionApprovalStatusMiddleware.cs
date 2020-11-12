using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using OwnIdSdk.NetCore3.Extensibility.Flow.Contracts;
using OwnIdSdk.NetCore3.Flow.Commands;
using OwnIdSdk.NetCore3.Flow.Interfaces;
using OwnIdSdk.NetCore3.Flow.Steps;
using OwnIdSdk.NetCore3.Web.Attributes;

namespace OwnIdSdk.NetCore3.Web.Middlewares.Approval
{
    [RequestDescriptor(BaseRequestFields.Context | BaseRequestFields.RequestToken | BaseRequestFields.ResponseToken)]
    public class GetActionApprovalStatusMiddleware : BaseMiddleware
    {
        private readonly IFlowRunner _flowRunner;

        public GetActionApprovalStatusMiddleware(RequestDelegate next, IFlowRunner flowRunner,
            ILogger<GetActionApprovalStatusMiddleware> logger, StopFlowCommand stopFlowCommand) : base(next, logger,
            stopFlowCommand)
        {
            _flowRunner = flowRunner;
        }

        protected override async Task Execute(HttpContext httpContext)
        {
            var result = await _flowRunner.RunAsync(
                new CommandInput(RequestIdentity, GetRequestCulture(httpContext), ClientDate),
                StepType.ApprovePin);

            await Json(httpContext, result, StatusCodes.Status200OK);
        }
    }
}