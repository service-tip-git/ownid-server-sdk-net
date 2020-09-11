using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Options;
using OwnIdSdk.NetCore3.Attributes;
using OwnIdSdk.NetCore3.Extensibility.Configuration.Profile;
using OwnIdSdk.NetCore3.Extensions;

namespace OwnIdSdk.NetCore3.Configuration.Profile
{
    /// <inheritdoc cref="IProfileConfiguration" />
    /// <summary>Implements mechanism of User Profile serialization and validation</summary>
    public class ProfileConfiguration : IProfileConfiguration
    {
        /// <summary>
        ///     Supported validation attributes map for OwnId application usage
        /// </summary>
        private static readonly Dictionary<Type, ProfileValidatorDescription> DataAnnotationAttrsMap =
            new Dictionary<Type, ProfileValidatorDescription>
            {
                {
                    typeof(RequiredAttribute), new ProfileValidatorDescription("required", "Field {0} is required")
                },
                {
                    typeof(MaxLengthAttribute), new ProfileValidatorDescription("maxlength",
                        "The field {0} must be a string or array type with a maximum length of '{1}'")
                },
                // add here MinLength, Regex etc.
            };

        /// <summary>
        ///     <see cref="ValidationAttribute"/> properties which must be excluded from <see cref="ProfileValidationRuleMetadata"/>
        /// </summary>
        private static readonly IList<string> BaseValidationAttributeProperties =
            typeof(ValidationAttribute).GetProperties().Select(x => x.Name).ToList();

        /// <summary>
        ///     Initializes instance of <see cref="ProfileConfiguration" />
        /// </summary>
        /// <param name="profileType">Sets value to <see cref="ProfileModelType" /></param>
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
                    $"Profile model type '{ProfileModelType.FullName}' must be non-abstract, non-generic class");

            var props = ProfileModelType.GetProperties();

            if (props.Length == 0)
                return ValidateOptionsResult.Fail(
                    $"Profile model type '{ProfileModelType.FullName}' must contain at least one public property with read/write access");


            var propertiesCount = 0;
            foreach (var propertyInfo in props)
            {
                if (propertyInfo.DeclaringType != null && (propertyInfo.PropertyType.IsPrimitive ||
                                                           propertyInfo.PropertyType == typeof(string) ||
                                                           propertyInfo.PropertyType == typeof(DateTime)))
                {
                    if (!propertyInfo.CanWrite)
                        Console.Out.WriteLine($"{propertyInfo.Name} has no setter method and will be skipped");
                    else
                        propertiesCount++;

                    continue;
                }

                if (propertyInfo.GetCustomAttributes<OwnIdFieldAttribute>().Any())
                    return ValidateOptionsResult.Fail($"{propertyInfo.Name} has unsupported type");

                Console.Out.WriteLine($"{propertyInfo.Name} has unsupported type and will be skipped");
            }

            if (propertiesCount == 0)
                return ValidateOptionsResult.Fail(
                    $"Profile model type '{ProfileModelType.FullName}' must have at least one property with setter");

            if (propertiesCount > 50)
                return ValidateOptionsResult.Fail(
                    $"Profile model type '{ProfileModelType.FullName}' must contain no more than 50 public properties with read/write access. It contain {propertiesCount}");

            return ValidateOptionsResult.Success;
        }

        public void BuildMetadata()
        {
            ProfileFieldMetadata = ProfileModelType
                .GetProperties().Select(GetField).ToList();
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
                var validator = new ProfileValidationRuleMetadata(typeAttr)
                {
                    Type = fieldData.Type,
                    ErrorKey = Constants.DefaultInvalidFormatErrorMessage,
                    NeedsInternalLocalization =
                        string.IsNullOrEmpty(typeAttr.ErrorMessageResourceName)
                        || typeAttr.ErrorMessageResourceType == null
                };

                fieldData.Validators.Add(validator);
            }

            return fieldData;
        }

        private ProfileValidationRuleMetadata GetFieldValidator(Type validatorType, MemberInfo memberInfo)
        {
            if (!(memberInfo.GetCustomAttributes(validatorType).FirstOrDefault() is ValidationAttribute
                validationAttribute))
                return null;

            var validatorDescription = DataAnnotationAttrsMap[validatorType];

            var validator = new ProfileValidationRuleMetadata(validationAttribute)
            {
                Type = validatorDescription.ClientSideTypeNaming,
                NeedsInternalLocalization =
                    string.IsNullOrEmpty(validationAttribute.ErrorMessageResourceName)
                    || validationAttribute.ErrorMessageResourceType == null
            };

            if (validator.NeedsInternalLocalization)
                validationAttribute.ErrorMessage = validatorDescription.DefaultErrorMessage;

            validator.ErrorKey = validationAttribute.ErrorMessage;


            foreach (var prop in validatorType.GetProperties())
            {
                if (BaseValidationAttributeProperties.Contains(prop.Name))
                    continue;

                var propValue = prop.GetValue(validationAttribute, null);
                if (propValue != null)
                    validator.Parameters.Add(prop.Name.ToCamelCase(), propValue.ToString());
            }

            return validator;
        }
    }
}