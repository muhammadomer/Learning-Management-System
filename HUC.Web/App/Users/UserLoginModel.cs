using System.ComponentModel.DataAnnotations;
using AtlasDB;
using HUC.Web.App.Shared;

namespace HUC.Web.App.Users
{
    [DBTableName("Users")]
    public class UserLoginModel : BaseModel
    {
        [Required, DataType(DataType.EmailAddress), StringLength(500), Display(Name = "Email")]
        public string Email { get; set; }

        [Required, DataType(DataType.Password), StringLength(500), Display(Name = "Password")]
        public string Password { get; set; }
    }
}