using System;
using System.ComponentModel.DataAnnotations;
using OwnIdSdk.NetCore3.Extensibility.Configuration.Profile;

namespace OwnIdSdk.NetCore3.Attributes
{
    /// <summary>
    ///     Validation attribute to specify decorated property format, add validation and change the display control on OwnId
    ///     application side,
    /// </summary>
    /// <remarks>
    ///     Has default error message <see cref="Constants.DefaultInvalidFormatErrorMessage" />
    /// </remarks>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class OwnIdFieldTypeAttribute : ValidationAttribute
    {
        /// <summary>
        /// </summary>
        /// <param name="profileFieldType">
        ///     Value for <see cref="FieldType" />. Determines property format with
        ///     <see cref="ProfileFieldType" />
        /// </param>
        public OwnIdFieldTypeAttribute(ProfileFieldType profileFieldType = ProfileFieldType.Text) : base(
            Constants.DefaultInvalidFormatErrorMessage)
        {
            FieldType = profileFieldType;
        }

        /// <summary>
        ///     Determines property format with <see cref="ProfileFieldType" />
        /// </summary>
        public ProfileFieldType FieldType { get; }

        /// <summary>
        ///     Validates provided value in context of <see cref="FieldType" />
        /// </summary>
        /// <param name="value">Property value</param>
        /// <returns>Returns <c>True</c> wherever <paramref name="value" /> is valid</returns>
        public override bool IsValid(object value)
        {
            return FieldType switch
            {
                ProfileFieldType.Text => true,
                ProfileFieldType.Email => new EmailAddressAttribute().IsValid(value),
                _ => true
            };
        }
    }
}