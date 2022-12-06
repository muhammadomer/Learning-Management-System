using System.ComponentModel.DataAnnotations;
using AtlasDB;
using HUC.Web.App.Shared;
using HUC.Web.App.Users;

namespace HUC.Web.App.Companies.Users
{
    public class CompanyUserAddModel : CompanyUserModel
    {
        [Required]
        [DBIgnore]
        public UserAddModel UserAdd { get; set; }
    }
}