using System;
using System.ComponentModel.DataAnnotations;
using FluentAssertions;
using OwnIdSdk.NetCore3.Attributes;
using OwnIdSdk.NetCore3.Configuration.Profile;
using OwnIdSdk.NetCore3.Extensibility.Configuration.Profile;
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
            config.Validate().Succeeded.Should().BeTrue();
        }

        [Theory]
        [InlineData(typeof(TestProfileModelNoProps))]
        [InlineData(typeof(TestProfileNoSetterProp))]
        [InlineData(typeof(TestProfileModelComplex))]
        [InlineData(typeof(TestProfileModelAbstract))]
        [InlineData(typeof(TestProfileModelGeneric<TestProfileModelValid>))]
        [InlineData(typeof(TestProfile51Fields))]
        public void Validate_Invalid(Type type)
        {
            var config = new ProfileConfiguration(type);
            config.Validate().Failed.Should().BeTrue();
        }

        [Fact]
        public void BuildMetadata_Test()
        {
            var config = new ProfileConfiguration(typeof(TestProfileModelAllAttr));
            config.BuildMetadata();

            config.ProfileModelType.Should().NotBeNull();
            config.ProfileModelType.Should().Be<TestProfileModelAllAttr>();
            config.ProfileFieldMetadata.Should().NotBeNull();
            config.ProfileFieldMetadata.Count.Should().Be(3);

            var age = config.ProfileFieldMetadata[0];
            age.Key.Should().Be("Age");
            age.Placeholder.Should().Be(string.Empty);
            age.Label.Should().Be("Age");
            age.Type.Should().Be("number");
            age.Validators.Should().ContainSingle();
            age.Validators[0].Type.Should().Be("number");

            var email = config.ProfileFieldMetadata[1];
            email.Key.Should().Be("Email");
            email.Placeholder.Should().Be("My Email Placeholder");

            email.Label.Should().Be("My Email");
            email.Type.Should().Be("email");
            email.Validators.Count.Should().Be(2);
            email.Validators.Should().Contain(x => x.Type == "required");
            email.Validators.Should().Contain(x => x.Type == "email");

            var name = config.ProfileFieldMetadata[2];
            name.Key.Should().Be("Name");
            name.Placeholder.Should().Be(string.Empty);
            name.Label.Should().Be("Name");
            name.Type.Should().Be("text");
            name.Validators.Should().BeEmpty();
        }
    }

    public class TestProfileModelValid
    {
        [Required]
        [OwnIdField("Emaii", "Email Placeholder")]
        public string Email { get; set; }

        public string Name { get; set; }

        public int Num { get; set; }

        public MyStruct Test { get; set; }

        public struct MyStruct
        {
        }
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

    public class TestProfile51Fields
    {
        public string Field1 { get; set; }
        public string Field2 { get; set; }
        public string Field3 { get; set; }
        public string Field4 { get; set; }
        public string Field5 { get; set; }
        public string Field6 { get; set; }
        public string Field7 { get; set; }
        public string Field8 { get; set; }
        public string Field9 { get; set; }
        public string Field10 { get; set; }
        public string Field11 { get; set; }
        public string Field12 { get; set; }
        public string Field13 { get; set; }
        public string Field14 { get; set; }
        public string Field15 { get; set; }
        public string Field16 { get; set; }
        public string Field17 { get; set; }
        public string Field18 { get; set; }
        public string Field19 { get; set; }
        public string Field20 { get; set; }
        public string Field21 { get; set; }
        public string Field22 { get; set; }
        public string Field23 { get; set; }
        public string Field24 { get; set; }
        public string Field25 { get; set; }
        public string Field26 { get; set; }
        public string Field27 { get; set; }
        public string Field28 { get; set; }
        public string Field29 { get; set; }
        public string Field30 { get; set; }
        public string Field31 { get; set; }
        public string Field32 { get; set; }
        public string Field33 { get; set; }
        public string Field34 { get; set; }
        public string Field35 { get; set; }
        public string Field36 { get; set; }
        public string Field37 { get; set; }
        public string Field38 { get; set; }
        public string Field39 { get; set; }
        public string Field40 { get; set; }
        public string Field41 { get; set; }
        public string Field42 { get; set; }
        public string Field43 { get; set; }
        public string Field44 { get; set; }
        public string Field45 { get; set; }
        public string Field46 { get; set; }
        public string Field47 { get; set; }
        public string Field48 { get; set; }
        public string Field49 { get; set; }
        public string Field50 { get; set; }
        public string Field51 { get; set; }
    }
}