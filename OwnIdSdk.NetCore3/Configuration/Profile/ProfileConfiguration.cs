using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Options;
using OwnIdSdk.NetCore3.Attributes;
using RequiredAttribute = System.ComponentModel.DataAnnotations.RequiredAttribute;

namespace OwnIdSdk.NetCore3.Configuration.Profile
{
    /// <inheritdoc cref="IProfileConfiguration"/>
    /// <summary>Implements mechanism of User Profile serialization and validation</summary>
    public class ProfileConfiguration : IProfileConfiguration
    {
        /// <summary>
        /// Localized field label placeholder
        /// </summary>
        private const string FieldNamePlaceholder = "{0}";
        
        /// <summary>
        /// Supported validation attributes map for OwnId application usage
        /// </summary>
        private static readonly Dictionary<Type, ProfileValidatorDescription> DataAnnotationAttrsMap = new Dictionary<Type, ProfileValidatorDescription>
        {
            {typeof(RequiredAttribute), new ProfileValidatorDescription("required", "Field {0} is required")}
            // add here MaxLength, MinLength, Regex
        };

        /// <summary>
        /// Initializes instance of <see cref="ProfileConfiguration"/>
        /// </summary>
        /// <param name="profileType">Sets value to <see cref="ProfileModelType"/></param>
        public ProfileConfiguration(Type profileType)
        {
            ProfileModelType = profileType;
        }

        public Type ProfileModelType { get; }

        public IReadOnlyList<ProfileFieldMetadata> ProfileFieldMetadata { get; private set; }

        public ValidateOptionsResult Validate()
        {
            if (!ProfileModelType.IsClass || ProfileModelType.IsAbstract || ProfileModelType.IsGenericType)
                return ValidateOptionsResult.Fail(
                    $"{nameof(ProfileModelType)} should be non-abstract, non-generic class");

            var props = ProfileModelType.GetProperties();

            if (props.Length == 0)
                return ValidateOptionsResult.Fail(
                    $"{nameof(ProfileModelType)} should contain at least one public property or field with read/write access");

            var hasAtLeastOneWritableProp = false;

            foreach (var propertyInfo in props)
            {
                if (propertyInfo.DeclaringType != null && (propertyInfo.PropertyType.IsPrimitive ||
                                                           propertyInfo.PropertyType == typeof(string) ||
                                                           propertyInfo.PropertyType == typeof(DateTime)))
                {
                    if (!propertyInfo.CanWrite)
                        Console.Out.WriteLine($"{propertyInfo.Name} has no setter method and will be skipped");
                    else
                        hasAtLeastOneWritableProp = true;

                    continue;
                }

                if (propertyInfo.GetCustomAttributes<OwnIdFieldAttribute>().Any())
                    return ValidateOptionsResult.Fail($"{propertyInfo.Name} has unsupported type");

                Console.Out.WriteLine($"{propertyInfo.Name} has unsupported type and will be skipped");
            }

            if (!hasAtLeastOneWritableProp)
                return ValidateOptionsResult.Fail(
                    $"{nameof(ProfileModelType)} should be have at least one property with setter");

            return ValidateOptionsResult.Success;
        }

        public void BuildMetadata()
        {
            var props = ProfileModelType.GetProperties();

            var metadata = new List<ProfileFieldMetadata>();

            foreach (var prop in props) metadata.Add(GetField(prop));

            ProfileFieldMetadata = metadata;
        }

        private ProfileFieldMetadata GetField(MemberInfo memberInfo)
        {
            var displayAttr = memberInfo.GetCustomAttributes<OwnIdFieldAttribute>().FirstOrDefault();
            var typeAttr = memberInfo.GetCustomAttributes<OwnIdFieldTypeAttribute>().FirstOrDefault();

            var fieldData = new ProfileFieldMetadata
            {
                Label = displayAttr?.Label ?? memberInfo.Name,
                Placeholder = displayAttr?.Placeholder ?? string.Empty,
                Key = memberInfo.Name,
                Type = (typeAttr?.FieldType ?? ProfileFieldType.Text).ToString().ToLowerInvariant(),
                Validators = new List<ProfileValidationRuleMetadata>()
            };

            foreach (var type in DataAnnotationAttrsMap.Keys)
            {
                var validator = GetFieldValidator(type, memberInfo);
                
                if (validator != null)
                    fieldData.Validators.Add(validator);
            }

            if (typeAttr != null && typeAttr.FieldType != ProfileFieldType.Text)
            {
                var validator = new ProfileValidationRuleMetadata
                {
                    Type = fieldData.Type,
                    GetErrorMessageKey = () => typeAttr.FormatErrorMessage(FieldNamePlaceholder),
                    NeedsInternalLocalization = string.IsNullOrEmpty(typeAttr.ErrorMessageResourceName) &&
                                                typeAttr.ErrorMessageResourceType == null
                };
                
                fieldData.Validators.Add(validator);
            }

            return fieldData;
        }

        private ProfileValidationRuleMetadata GetFieldValidator(Type type, MemberInfo memberInfo)
        {
            if (!type.IsSubclassOf(typeof(ValidationAttribute)))
                return null;

            if (memberInfo.GetCustomAttributes(type).FirstOrDefault() is ValidationAttribute attribute)
            {
                var validatorDescription = DataAnnotationAttrsMap[type];
                
                var validator = new ProfileValidationRuleMetadata
                {
                    Type = validatorDescription.ClientSideTypeNaming
                };

                if (!string.IsNullOrEmpty(attribute.ErrorMessageResourceName) &&
                    attribute.ErrorMessageResourceType != null)
                {
                    validator.NeedsInternalLocalization = false;
                }
                else
                {
                    if (string.IsNullOrEmpty(attribute.ErrorMessage))
                    {
                        attribute.ErrorMessage = validatorDescription.DefaultErrorMessage;
                    }

                    validator.NeedsInternalLocalization = true;
                }
                
                validator.GetErrorMessageKey = () => attribute.FormatErrorMessage(FieldNamePlaceholder);

                return validator;
            }

            return null;
        }
    }
}