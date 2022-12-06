using System.ComponentModel.DataAnnotations;
using HUC.Web.App.Shared;

namespace HUC.Web.App.Users
{
    public class UserResetPasswordModel : BaseModel
    {
        public string Code { get; set; }

        [Required]
        [StringLength(500)]
        [Display(Name = "Confirm Password")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }

        [Required]
        [StringLength(500)]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}