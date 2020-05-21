using System;
using System.ComponentModel.DataAnnotations;
using OwnIdSdk.NetCore3.Attributes;
using OwnIdSdk.NetCore3.Configuration.Profile;
using Xunit;

namespace OwnIdSdk.NetCore3.Tests.Configuration
{
    public class ProfileConfigurationTest
    {
        [Theory]
        [InlineData(typeof(TestProfileModelValid))]
        [InlineData(typeof(TestProfileModelAllAttr))]
        public void Validate_Valid(Type type)
        {
            var config = new ProfileConfiguration(type);
            Assert.True(config.Validate().Succeeded);
        }
        
        [Theory]
        [InlineData(typeof(TestProfileModelNoProps))]
        [InlineData(typeof(TestProfileNoSetterProp))]
        [InlineData(typeof(TestProfileModelComplex))]
        [InlineData(typeof(TestProfileModelAbstract))]
        [InlineData(typeof(TestProfileModelGeneric<TestProfileModelValid>))]
        public void Validate_Invalid(Type type)
        {
            var config = new ProfileConfiguration(type);
            Assert.True(config.Validate().Failed, $"{type.Name} check failed");
        }

        [Fact]
        public void BuildMetadata_Test()
        {
            var config = new ProfileConfiguration(typeof(TestProfileModelAllAttr));
            config.BuildMetadata();
            
            Assert.NotNull(config.ProfileModelType);
            Assert.Equal(typeof(TestProfileModelAllAttr), config.ProfileModelType);
            Assert.NotNull(config.ProfileFieldMetadata);
            Assert.Equal(3, config.ProfileFieldMetadata.Count);

            var age = config.ProfileFieldMetadata[0];
            Assert.Equal("Age", age.Key);
            Assert.Equal(string.Empty, age.Placeholder);
            Assert.Equal("Age", age.Label);
            Assert.Equal("number", age.Type);
            Assert.Single(age.Validators);
            Assert.Equal("number", age.Validators[0].Type);

            var email = config.ProfileFieldMetadata[1];
            Assert.Equal("Email", email.Key);
            Assert.Equal("My Email Placeholder", email.Placeholder);
            Assert.Equal("My Email", email.Label);
            Assert.Equal("email", email.Type);
            Assert.Equal(2, email.Validators.Count);
            Assert.Contains(email.Validators, x => x.Type == "required");
            Assert.Contains(email.Validators, x => x.Type == "email");

            var name = config.ProfileFieldMetadata[2];
            Assert.Equal("Name", name.Key);
            Assert.Equal(string.Empty, name.Placeholder);
            Assert.Equal("Name", name.Label);
            Assert.Equal("text", name.Type);
            Assert.Empty(name.Validators);
        }
    }

    public class TestProfileModelValid
    {
        [Required]
        [OwnIdField("Emaii", "Email Placeholder")]
        public string Email { get; set; }
        
        public string Name { get; set; }
    }

    public class TestProfileModelNoProps
    {
        public string Name;
    }

    public class TestProfileNoSetterProp
    {
        public string Email { get; }
        
        public string Name { get; }
    }

    public class TestProfileModelComplex
    {
        public string Email { get; set; }
        
        [OwnIdField("Complex type", "complex Type placeholder")]
        public TestProfileModelNoProps Complextype { get; set; }
    }

    public abstract class TestProfileModelAbstract
    {
        public string Email { get; set; }
    }

    public class TestProfileModelGeneric<T>
    {
        public string Email { get; set; }
        public T ParamType { get; set; }
    }

    public class TestProfileModelAllAttr
    {
        [OwnIdFieldType(ProfileFieldType.Number)]
        public int Age { get; set; }
        
        [Required]
        [OwnIdField("My Email", "My Email Placeholder")]
        [OwnIdFieldType(ProfileFieldType.Email)]
        public string Email { get; set; }

        public string Name { get; set; }
    }
}