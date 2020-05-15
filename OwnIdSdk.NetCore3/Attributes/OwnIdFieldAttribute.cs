using System;
using OwnIdSdk.NetCore3.Configuration;

namespace OwnIdSdk.NetCore3.Attributes
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class OwnIdFieldAttribute : Attribute
    {
        public OwnIdFieldAttribute(string label, string placeholder)
        {
            Label = label;
            Placeholder = placeholder;
        }

        public string Label { get; }
        public string Placeholder { get; }
    }
}