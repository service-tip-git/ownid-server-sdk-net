using System;

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