using System;
using System.ComponentModel.DataAnnotations;
using OwnIdSdk.NetCore3.Configuration;

namespace OwnIdSdk.NetCore3.Attributes
{
    /// <summary>
    /// Format validation attribute
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class OwnIdFieldTypeAttribute : ValidationAttribute
    {
        public OwnIdFieldTypeAttribute(ProfileFieldType profileFieldType = ProfileFieldType.Text)
        {
            FieldType = profileFieldType;
        }

        public override bool IsValid(object value)
        {
            if (FieldType == ProfileFieldType.Text)
                return true;

            if (FieldType == ProfileFieldType.Email)
                return new EmailAddressAttribute().IsValid(value);

            return true;
        }

        public ProfileFieldType FieldType { get; }
    }
}