using System.ComponentModel.DataAnnotations;
using AtlasDB;
using HUC.Web.App.Shared;
using HUC.Web.App.Users;

namespace HUC.Web.App.Companies.Users
{
    public class CompanyUserModel : BaseModel
    {
        public int CompanyID { get; set; }
        public int UserID { get; set; }

        [Display(Name = "Is Administrator")]
        public bool IsAdmin { get; set; }

        [Display(Name = "Is Backup Administrator")]
        public bool IsBackupAdmin { get; set; }

        [Display(Name = "Can this backup admin use the courses?")]
        public bool IsBackupAdminCourseUsable { get; set; }

        //Lazy
        private UserModel _user;
        private CompanyModel _company;
        public int? HasFreeText;
        public int IsIssueCertificate;

        [DBIgnore]
        public UserModel User
        {
            get
            {
                if (_user == null)
                {
                    _user = Database.GetSingle<UserModel>(this.UserID);
                }
                return _user;
            }
            set { _user = value; }
        }

        [DBIgnore]
        public CompanyModel Company
        {
            get
            {
                if (_company == null)
                {
                    _company = Database.GetSingle<CompanyModel>(this.CompanyID);
                }
                return _company;
            }
            set { _company = value; }
        }
    }
}