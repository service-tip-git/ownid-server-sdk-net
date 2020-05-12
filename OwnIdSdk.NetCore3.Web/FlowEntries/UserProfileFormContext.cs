using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using OwnIdSdk.NetCore3.Configuration;
using OwnIdSdk.NetCore3.Contracts.Jwt;

namespace OwnIdSdk.NetCore3.Web.FlowEntries
{
    public class UserProfileFormContext : ReadOnlyDictionary<string, FieldContext<string>>
    {
        internal UserProfileFormContext(UserProfile userProfile, IEnumerable<ProfileField> fieldConfigs) : base(
            Init(userProfile.Profile, fieldConfigs))
        {
            DID = userProfile.DID;
            PublicKey = userProfile.PublicKey;
            GeneralErrors = new List<string>();
        }

        internal IList<string> GeneralErrors { get; }

        public string DID { get; }

        public string PublicKey { get; }

        public bool HasErrors => GeneralErrors.Any() ||
                                 Values.Any(x => x.IsInvalid);

        public void SetError(string key, string error)
        {
            this[key].Errors.Add(error);
        }

        public void SetGeneralError(string error)
        {
            GeneralErrors.Add(error);
        }

        private static Dictionary<string, FieldContext<string>> Init(Dictionary<string, string> userProfile,
            IEnumerable<ProfileField> fieldConfigs)
        {
            var result = new Dictionary<string, FieldContext<string>>();

            foreach (var fieldConfig in fieldConfigs)
            {
                userProfile.TryGetValue(fieldConfig.Key, out var value);
                var field = new FieldContext<string>(fieldConfig, value);
                result.Add(fieldConfig.Key, field);
            }

            return result;
        }
    }
}