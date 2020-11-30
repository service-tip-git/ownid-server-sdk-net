using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using OwnID.Extensibility.Flow.Contracts;
using OwnID.Extensibility.Json;
using OwnID.Flow.Commands;
using OwnID.Flow.Commands.Approval;
using OwnID.Web.Attributes;

namespace OwnID.Web.Middlewares.Approval
{
    [RequestDescriptor(BaseRequestFields.Context)]
    public class ApproveActionMiddleware : BaseMiddleware
    {
        private readonly ApproveActionCommand _approveActionCommand;

        public ApproveActionMiddleware(RequestDelegate next, ApproveActionCommand approveActionCommand,
            ILogger<ApproveActionMiddleware> logger, StopFlowCommand stopFlowCommand)
            : base(next, logger, stopFlowCommand)
        {
            _approveActionCommand = approveActionCommand;
        }

        protected override async Task Execute(HttpContext httpContext)
        {
            var request = await OwnIdSerializer.DeserializeAsync<ApproveActionRequest>(httpContext.Request.Body);

            await _approveActionCommand.ExecuteAsync(request);
            OkNoContent(httpContext.Response);
        }
    }
}