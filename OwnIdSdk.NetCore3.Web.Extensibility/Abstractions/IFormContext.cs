using System.Collections.Generic;

namespace OwnIdSdk.NetCore3.Web.Extensibility.Abstractions
{
    public interface IFormContext
    {
        public List<string> GeneralErrors { get; set; }

        public IReadOnlyDictionary<string, IList<string>> FieldErrors { get; }

        void Validate();
        
        bool HasErrors { get; }

        void SetGeneralError(string error);
    }
}