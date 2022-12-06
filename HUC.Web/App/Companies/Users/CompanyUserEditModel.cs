using System.ComponentModel.DataAnnotations;
using AtlasDB;
using HUC.Web.App.Shared;
using HUC.Web.App.Users;

namespace HUC.Web.App.Companies.Users
{
    public class CompanyUserEditModel : CompanyUserModel
    {
        [Required]
        [DBIgnore]
        public UserEditModel UserEdit { get; set; }
    }
}