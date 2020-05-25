using System.ComponentModel.DataAnnotations;
using OwnIdSdk.NetCore3.Attributes;
using OwnIdSdk.NetCore3.Configuration.Profile;
using OwnIdSdk.NetCore3.Server.Gigya.Resources;

namespace OwnIdSdk.NetCore3.Server.Gigya
{
    public class UserProfile
    {
        [OwnIdField(nameof(Email), "Email")]
        [OwnIdFieldType(ProfileFieldType.Email)]
        [Required]
        public string Email { get; set; }

        [Required]
        [OwnIdField("First Name", "First Name")]
        public string FirstName { get; set; }

        [Required]
        [OwnIdField("Last Name", "Last Name")]
        public string LastName { get; set; }

        [OwnIdField("My Nickname", "My Nickname")]
        public string Nickname { get; set; }
    }
}