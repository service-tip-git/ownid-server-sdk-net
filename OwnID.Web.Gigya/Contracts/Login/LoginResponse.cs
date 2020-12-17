using System.Collections.Generic;

namespace OwnID.Web.Gigya.Contracts.Login
{
    public class LoginResponse : BaseGigyaResponse
    {
        public Dictionary<string, string> SessionInfo { get; set; }

        public IList<Identity> Identities { get; set; }
    }
}