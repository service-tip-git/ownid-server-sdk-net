using System.ComponentModel.DataAnnotations;
using OwnID.Attributes;
using OwnID.Extensibility.Configuration.Profile;

namespace OwnID.Configuration.Profile
{
    /// <summary>
    ///     Default User Profile representation
    /// </summary>
    public class DefaultProfileModel
    {
        [OwnIdField(Constants.DefaultEmailLabel, Constants.DefaultEmailLabel)]
        [OwnIdFieldType(ProfileFieldType.Email)]
        [Required]
        [MaxLength(200)]
        public string Email { get; set; }

        [OwnIdField(Constants.DefaultFirstNameLabel, Constants.DefaultFirstNameLabel)]
        [Required]
        [MaxLength(200)]
        public string FirstName { get; set; }

        [OwnIdField(Constants.DefaultLastNameLabel, Constants.DefaultLastNameLabel)]
        [Required]
        [MaxLength(200)]
        public string LastName { get; set; }
    }
}