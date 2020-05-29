using System.ComponentModel.DataAnnotations;
using OwnIdSdk.NetCore3.Attributes;

namespace OwnIdSdk.NetCore3.Configuration.Profile
{
    public class DefaultProfileModel
    {
        [OwnIdField(Constants.DefaultEmailLabel, Constants.DefaultEmailLabel)]
        [OwnIdFieldType(ProfileFieldType.Email)]
        [Required]
        public string Email { get; set; }
        
        [OwnIdField(Constants.DefaultFirstNameLabel, Constants.DefaultFirstNameLabel)]
        [Required]
        public string FirstName { get; set; }
        
        [OwnIdField(Constants.DefaultLastNameLabel, Constants.DefaultLastNameLabel)]
        [Required]
        public string LastName { get; set; }
    }
}