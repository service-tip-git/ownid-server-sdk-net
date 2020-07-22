using OwnIdSdk.NetCore3.Extensibility.Cache;
using OwnIdSdk.NetCore3.Extensibility.Flow.Contracts.Jwt;

namespace OwnIdSdk.NetCore3.Extensibility.Flow.Contracts.Approval
{
    public class GetApprovalStatusResponse : JwtContainer
    {
        public GetApprovalStatusResponse()
        {
        }

        public GetApprovalStatusResponse(string jwt, CacheItemStatus cacheItemStatus) : base(jwt)
        {
            Status = cacheItemStatus;
        }

        public CacheItemStatus Status { get; set; }
    }
}