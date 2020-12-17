using System.Collections.Generic;

namespace OwnID.Web.Gigya.Contracts.Accounts
{
    public class GetAccountInfoResponseList<TProfile> : BaseGigyaResponse where TProfile : class, IGigyaUserProfile
    {
        public List<GetAccountInfoResponse<TProfile>> Results { get; set; }
    }
}