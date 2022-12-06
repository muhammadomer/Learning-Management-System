using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using AtlasDB;
using HUC.Web.App.Courses;
using HUC.Web.App.Shared;

namespace HUC.Web.App.Companies.Users.Courses
{
    public class DirectCourseAddModel : BaseModel
    {
        [Required]
        public int CompanyUserID { get; set; }
        [Required]
        public int CourseID { get; set; }

        [Required, DisplayName("Compliance Score Minimum")]
        public int? ComplianceScoreMinimum { get; set; }

        public List<SelectListItem> CourseOptions()
        {
            var sli = new List<SelectListItem>();
            sli.Add(new SelectListItem());

            var options = Database.GetAll<CourseModel>(
                "WHERE IsDeleted = 0 AND IsPublished = 1 " +
                "AND ID NOT IN ( " +
                "   SELECT CourseID FROM CompanyCourses " +
                "   WHERE CompanyID = ( " +
                "       SELECT CompanyID FROM CompanyUsers WHERE ID = @CompanyUserID " +
                "   ) " +
                ") " +
                "AND ID NOT IN ( " +
                "   SELECT CourseID FROM UserCourses " +
                "   WHERE UserID = ( " +
                "       SELECT UserID FROM CompanyUsers WHERE ID = @CompanyUserID " +
                "   ) " +
                ") ",new
                {
                    CompanyUserID = CompanyUserID
                });
            foreach (var curOption in options)
            {
                sli.Add(new SelectListItem
                {
                    Text = curOption.Name,
                    Value = curOption.ID.ToString()
                });
            }

            return sli;
        }

        private CompanyUserModel _companyUser;
        [DBIgnore]
        public CompanyUserModel CompanyUser
        {
            get
            {
                if (_companyUser == null)
                {
                    _companyUser = Database.GetSingle<CompanyUserModel>(CompanyUserID);
                }
                return _companyUser;
            }
            set { _companyUser = value; }
        }
    }
}