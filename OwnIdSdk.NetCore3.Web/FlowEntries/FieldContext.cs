using System.Collections.Generic;
using System.Linq;
using OwnIdSdk.NetCore3.Configuration;

namespace OwnIdSdk.NetCore3.Web.FlowEntries
{
    public class FieldContext<T>
    {
        internal FieldContext(ProfileField configuration, T value = default)
        {
            Errors = new List<string>();
            Value = value;
            FieldConfiguration = configuration;
        }

        public string Key => FieldConfiguration.Key;

        public T Value { get; }

        public ProfileField FieldConfiguration { get; }

        public bool IsInvalid => Errors.Any();

        public List<string> Errors { get; }
    }
}