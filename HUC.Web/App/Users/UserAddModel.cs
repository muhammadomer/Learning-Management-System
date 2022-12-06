using System.ComponentModel.DataAnnotations;
using AtlasDB;

namespace HUC.Web.App.Users
{
    public class UserAddModel : UserModel
    {
        private string _password;

        [Required]
        [StringLength(500)]
        [Display(Name = "Confirm Password")]
        [DataType(DataType.Password)]
        [DBIgnore]
        public string ConfirmPassword { get; set; }

        [Required]
        [StringLength(500)]
        [DataType(DataType.Password)]
        public string Password
        {
            get { return _password; }
            set
            {
                base.Password = value;
                _password = value;
            }
        }
    }
}