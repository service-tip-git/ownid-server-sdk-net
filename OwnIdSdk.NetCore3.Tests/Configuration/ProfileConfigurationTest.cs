using System;
using System.ComponentModel.DataAnnotations;
using OwnIdSdk.NetCore3.Attributes;
using OwnIdSdk.NetCore3.Configuration;
using Xunit;

namespace OwnIdSdk.NetCore3.Tests.Configuration
{
    public class ProfileConfigurationTest
    {
        [Fact]
        public void Validate_Valid()
        {
            var config = new ProfileConfiguration(typeof(TestProfileModelValid));
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
}