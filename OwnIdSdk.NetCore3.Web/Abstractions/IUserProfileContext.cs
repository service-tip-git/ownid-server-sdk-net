using System.Collections.Generic;

namespace OwnIdSdk.NetCore3.Web.Abstractions
{
    public interface IUserProfileContext
    {
        public List<string> GeneralErrors { get; set; }

        public IReadOnlyDictionary<string, IList<string>> FieldErrors { get; }

        void Validate();
        
        bool HasErrors { get; }
    }
}