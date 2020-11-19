using System.Collections.Generic;

namespace OwnID.Web.Gigya.Contracts.Accounts
{
    public class GetAccountInfoResponseList<TProfile> where TProfile : class, IGigyaUserProfile
    {
        public List<GetAccountInfoResponse<TProfile>> Results { get; set; }
    }
}