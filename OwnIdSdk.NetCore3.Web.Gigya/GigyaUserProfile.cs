using System.ComponentModel.DataAnnotations;
using OwnIdSdk.NetCore3.Attributes;
using OwnIdSdk.NetCore3.Configuration.Profile;

namespace OwnIdSdk.NetCore3.Web.Gigya
{
    public class GigyaUserProfile
    {
        [OwnIdField(Constants.DefaultEmailLabel, Constants.DefaultEmailLabel)]
        [OwnIdFieldType(ProfileFieldType.Email)]
        [Required]
        public string Email { get; set; }
        
        [OwnIdField(Constants.DefaultFirstNameLabel, Constants.DefaultFirstNameLabel)]
        public string FirstName { get; set; }
        
        [OwnIdField(Constants.DefaultLastNameLabel, Constants.DefaultLastNameLabel)]
        public string LastName { get; set; }
    }
}