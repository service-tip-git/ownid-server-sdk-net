using System;

namespace OwnIdSdk.NetCore3.Attributes
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class OwnIdFieldAttribute : Attribute
    {
        /// <param name="label">Label text that will be localized by IStringLocalizer from configuration</param>
        /// <param name="placeholder">Place holder that will be localized by IStringLocalizer from configuration</param>
        public OwnIdFieldAttribute(string label, string placeholder = null)
        {
            Label = label;
            Placeholder = placeholder;
        }

        public string Label { get; }
        public string Placeholder { get; }
    }
}