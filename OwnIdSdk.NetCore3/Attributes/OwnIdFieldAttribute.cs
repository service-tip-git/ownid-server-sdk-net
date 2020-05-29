using System;
using System.Diagnostics.CodeAnalysis;

namespace OwnIdSdk.NetCore3.Attributes
{
    /// <summary>
    ///     Attribute to configure User Profile properties display options (label, placeholder)
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class OwnIdFieldAttribute : Attribute
    {
        /// <param name="label">
        ///     Value for <see cref="Label" />. Label text that will be localized by value (as resource key) with
        ///     <see cref="OwnIdSdk.NetCore3.ILocalizationService" />
        /// </param>
        /// <param name="placeholder">
        ///     Value for <see cref="Placeholder" />. Placeholder that will be localized by value (as resource key) with
        ///     <see cref="OwnIdSdk.NetCore3.ILocalizationService" />
        /// </param>
        public OwnIdFieldAttribute([NotNull] string label, string placeholder = null)
        {
            Label = label;
            Placeholder = placeholder;
        }

        /// <summary>
        ///     Label text that will be localized by value (as resource key) with <see cref="OwnIdSdk.NetCore3.ILocalizationService" />
        /// </summary>
        public string Label { get; }

        /// <summary>
        ///     Place holder that will be localized by value (as resource key) with <see cref="OwnIdSdk.NetCore3.ILocalizationService" />
        /// </summary>
        public string Placeholder { get; }
    }
}