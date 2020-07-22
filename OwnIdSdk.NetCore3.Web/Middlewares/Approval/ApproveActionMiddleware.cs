using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using OwnIdSdk.NetCore3.Extensibility.Flow.Contracts;
using OwnIdSdk.NetCore3.Flow.Commands.Approval;
using OwnIdSdk.NetCore3.Web.Attributes;

namespace OwnIdSdk.NetCore3.Web.Middlewares.Approval
{
    [RequestDescriptor(BaseRequestFields.Context)]
    public class ApproveActionMiddleware : BaseMiddleware
    {
        private readonly ApproveActionCommand _approveActionCommand;

        public ApproveActionMiddleware(RequestDelegate next, ApproveActionCommand approveActionCommand,
            ILogger<ApproveActionMiddleware> logger) : base(next, logger)
        {
            _approveActionCommand = approveActionCommand;
        }

        protected override async Task Execute(HttpContext httpContext)
        {
            var request = await JsonSerializer.DeserializeAsync<ApproveActionRequest>(httpContext.Request.Body);

            await _approveActionCommand.ExecuteAsync(request);
            OkNoContent(httpContext.Response);
        }
    }
}