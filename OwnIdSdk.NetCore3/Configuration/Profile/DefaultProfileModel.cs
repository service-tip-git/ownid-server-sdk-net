using System.ComponentModel.DataAnnotations;
using OwnIdSdk.NetCore3.Attributes;
using OwnIdSdk.NetCore3.Extensibility.Configuration.Profile;

namespace OwnIdSdk.NetCore3.Configuration.Profile
{
    /// <summary>
    ///     Default User Profile representation
    /// </summary>
    public class DefaultProfileModel
    {
        [OwnIdField(Constants.DefaultEmailLabel, Constants.DefaultEmailLabel)]
        [OwnIdFieldType(ProfileFieldType.Email)]
        [Required]
        [MaxLength(50)]
        public string Email { get; set; }

        [OwnIdField(Constants.DefaultFirstNameLabel, Constants.DefaultFirstNameLabel)]
        [Required]
        [MaxLength(50)]
        public string FirstName { get; set; }

        [OwnIdField(Constants.DefaultLastNameLabel, Constants.DefaultLastNameLabel)]
        [Required]
        [MaxLength(50)]
        public string LastName { get; set; }
    }
}