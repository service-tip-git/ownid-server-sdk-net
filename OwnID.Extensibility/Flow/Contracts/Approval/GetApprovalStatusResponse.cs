using OwnID.Extensibility.Cache;
using OwnID.Extensibility.Flow.Contracts.Jwt;

namespace OwnID.Extensibility.Flow.Contracts.Approval
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