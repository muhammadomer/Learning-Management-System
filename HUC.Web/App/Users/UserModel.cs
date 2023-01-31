using AtlasDB;
using HUC.Web.App.Companies;
using HUC.Web.App.Courses;
using HUC.Web.App.Shared;
using HUC.Web.App.Users.Courses;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;


namespace HUC.Web.App.Users
{
    public class UserModel : BaseModel
    {
        public int ID { get; set; }
        public int? LastUserCourseID { get; set; }

        [Required, StringLength(500), DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        public string Password { get; set; }
        public string Salt { get; set; }

        [Required, StringLength(500), DataType(DataType.Text)]
        public string FirstName { get; set; }
        [Required, StringLength(500), DataType(DataType.Text)]
        public string LastName { get; set; }

        public string ActivateKey { get; set; }
        public string ResetPasswordKey { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        

        [DBIgnore]
        [Display(Name = "Roles")]
        public IEnumerable<int> RoleIDs { get; set; }

        //Functions

        [DBIgnore]
        public List<SelectListItem> RoleOptions()
        {
            var sli = new List<SelectListItem>();

            var roles = Database.GetAll<RoleModel>();
            foreach (var curRole in roles)
            {
                sli.Add(new SelectListItem
                {
                    Text = curRole.Name,
                    Value = curRole.ID.ToString()
                });
            }

            return sli;
        }

        //Lazy Items
        private IEnumerable<RoleModel> _roles;
        private CompanyModel _company;
        private bool _isCompanyAdmin;
        private IEnumerable<DateTime> _logins;
        private decimal? _totalCompliancePercent;
        private UserCourseModel _lastUserCoruse;

        [DBIgnore]
        public IEnumerable<RoleModel> Roles
        {
            get
            {
                if (_roles == null)
                {
                    _roles = Database.GetAll<RoleModel>("WHERE ID IN (SELECT RoleID FROM UserRoles WHERE UserID = @ID)", new { ID = this.ID });
                }
                return _roles;
            }
            set { _roles = value; }
        }

        /// <summary>
        /// This is the company that the user is DIRECTLY assigned to
        /// </summary>
        [DBIgnore]
        public CompanyModel Company
        {
            get
            {
                if (_company == null)
                {
                    _company = Database.GetAll<CompanyModel>("WHERE ID IN (SELECT CompanyID FROM CompanyUsers WHERE UserID = @ID)", new { ID = this.ID }).SingleOrDefault();
                }
                return _company;
            }
            set { _company = value; }
        }

        private IEnumerable<CompanyModel> _representableCompanies;
        /// <summary>
        /// These are the companies that the user can choose to represent
        /// </summary>
        [DBIgnore]
        public IEnumerable<CompanyModel> RepresentableCompanies
        {
            get
            {
                if (_representableCompanies == null && Company != null)
                   // if (_representableCompanies == null )
                    {
                    _representableCompanies = Database.Query<CompanyModel>(
                       @"
WITH cte AS 
(
	SELECT *
	FROM Companies a
	WHERE ID = @Id
	UNION ALL
	SELECT a.*
	FROM Companies a JOIN cte c ON a.ParentCompanyID = c.ID
)
SELECT *
FROM cte;
", new { ID = Company.ID }
                        );
                }
                return _representableCompanies;
            }
            set { _representableCompanies = value; }
        }

        [DBIgnore]
        public bool HasRole(string role)
        {
            return this.Roles.Any(x => x.Name.ToLower() == role.ToLower());
        }

        [DBIgnore]
        public IEnumerable<CourseModel> StartedCourses
        {
            get
            {
                var startedCourses = Database.Query<CourseModel>(
                    "SELECT Courses.* FROM Courses INNER JOIN AssignedCourses ac on ac.CourseID = Courses.ID WHERE ac.UserID = @UserID AND Courses.ID IN (SELECT CourseID FROM UserCourses WHERE UserID = @UserID AND IsComplete = 0 AND StartedOn IS NOT NULL)",
                    new { UserID = ID, CompanyID = this.ID });
                return startedCourses;
                //Tahir001
                //var startedCourses = Database.Query<CourseModel>(
                //    "SELECT * FROM Courses WHERE ID IN (SELECT CourseID FROM UserCourses WHERE UserID = @UserID AND IsComplete = 0 AND StartedOn IS NOT NULL)",
                //    new { UserID = ID, CompanyID = this.ID });
                //return startedCourses;
            }
        }

        [DBIgnore]
        public IEnumerable<CourseModel> CoursesReadyToStart
        {
            get
            {
                //Available courses are all company courses that haven't been started or completed and all user courses that haven't been started
                var available = new List<CourseModel>();

                if (this.Company != null)
                {
                    var assignedCourses = new UsersService().GetAssignedCourses(this.ID);
                    var allCourses = this.Company.Courses.Where(x => assignedCourses.Any( z => x.CourseID == z));
                    //.Where(x => courselList.Any(z => x.ID == z)).ToList();
                    //Tahir001
                    //var allCourses = this.Company.Courses;

                    available.AddRange(
                        allCourses.Where(x => StartedCourses.All(y => y.ID != x.ID) && CompletedCourses.All(y => y.ID != x.ID)).Select(x => x.Course)
                    );
                }

                available.AddRange(Database.Query<CourseModel>(
                    "SELECT Courses.* FROM Courses  INNER JOIN AssignedCourses ac on ac.CourseID = Courses.ID " +
                    "WHERE ac.UserID = @UserID AND Courses.ID IN (" +
                    "   SELECT CourseID FROM UserCourses WHERE StartedOn IS NULL AND UserID = @UserID" +
                    ") " +
                    (available.Any() ? "AND Courses.ID NOT IN (" + String.Join(", ", available.Select(x => x.ID)) + ")" : ""),
                    new { UserID = this.ID }));

                return available;
            }
        }

        [DBIgnore]
        public IEnumerable<CourseModel> AllCourses(int? year = null)
        {
            
                var all = new List<CourseModel>();

                if (this.Company != null && !year.HasValue)
                {
                var assignedCourses = new UsersService().GetAssignedCourses(this.ID);
                //var allCourses = this.Company.Courses.Where(x => assignedCourses.Any(z => x.CourseID == z));
                all.AddRange(Company.Courses.Select(x => x.Course).Where(x => assignedCourses.Any(z => x.ID == z)));
                //all.AddRange(Company.Courses.Select(x => x.Course));
                }

                var userEnteredCourses = Database.Query<CourseModel>(
                    "SELECT Courses.* FROM Courses  INNER JOIN AssignedCourses ac on ac.CourseID = Courses.ID " +
                    "WHERE ac.UserID = @UserID AND Courses.ID IN (" +
                        "   SELECT CourseID FROM UserCourses WHERE UserID = @UserID" +
                        (year.HasValue ? $" AND year(StartedOn) = {year}" : "") +
                        ") "
                        +
                        (all.Any() ? "AND Courses.ID NOT IN (" + String.Join(", ", all.Select(x => x.ID)) + ")" : "")
                        ,
                        new {UserID = this.ID});
                    
                   all.AddRange(userEnteredCourses);
            //var userEnteredCourses = Database.Query<CourseModel>(
            //            "SELECT * FROM Courses " +
            //            "WHERE ID IN (" +
            //            "   SELECT CourseID FROM UserCourses WHERE UserID = @UserID" +
            //            (year.HasValue ? $" AND year(StartedOn) = {year}" : "") +
            //            ") "
            //            +
            //            (all.Any() ? "AND ID NOT IN (" + String.Join(", ", all.Select(x => x.ID)) + ")" : "")
            //            ,
            //            new {UserID = this.ID});
                    
            //       all.AddRange(userEnteredCourses);
                    
                return all;
        }

        [DBIgnore]
        public IEnumerable<CourseModel> CompletedCourses
        {
            get
            {
                var completedCourses = Database.Query<CourseModel>(
                    "SELECT Courses.* FROM Courses  INNER JOIN AssignedCourses ac on ac.CourseID = Courses.ID " +
                    "WHERE ac.UserID = @UserID AND Courses.ID IN (SELECT CourseID FROM UserCourses WHERE UserID = @UserID AND IsComplete = 1)",
                    new { UserID = ID, CompanyID = this.ID });
                return completedCourses;
            }
        }

        [DBIgnore]
        public IEnumerable<DateTime> Logins
        {
            get
            {
                if (_logins == null)
                {
                    _logins = Database.Query<DateTime>("SELECT LoginOn FROM UserLogins WHERE UserID = @ID order by LoginOn desc",
                        new { ID = this.ID });
                }
                return _logins;
            }
            set { _logins = value; }
        }


        [DBIgnore]
        public decimal TotalCompliancePercent(int ? year = null)
        {
            
                if (!_totalCompliancePercent.HasValue)
                {
                    if (this.Company == null)
                    {
                        _totalCompliancePercent = 0m;
                    }
                    else
                    {
                        var courses = AllCourses(year).ToList();
                        var totalCourses = courses.Count();

                        if (totalCourses == 0)
                        {
                            _totalCompliancePercent = 0;
                        }
                        else
                        {
                            var totalPassedCourses = 0;

                            foreach (var curCourse in courses)
                            {
                                var userCourse = UserCourses.FirstOrDefault(x => x.CourseID == curCourse.ID);

                                if (userCourse != null)
                                {
                                    if (userCourse.TotalScore > userCourse.ComplianceScoreMinimum)
                                    {
                                        totalPassedCourses++;
                                    }
                                }
                            }

                            _totalCompliancePercent = ((decimal)(totalPassedCourses * 100) / totalCourses);
                        }
                    }
                }
                return _totalCompliancePercent.Value;
        }

        [DBIgnore]
        public UserCourseModel LastUserCourse
        {
            get
            {
                if (_lastUserCoruse == null)
                {
                    _lastUserCoruse = Database.GetSingle<UserCourseModel>(this.LastUserCourseID);
                }
                return _lastUserCoruse;
            }
            set { _lastUserCoruse = value; }
        }

        private IEnumerable<UserCourseModel> _userCourses;
        [DBIgnore]
        public IEnumerable<UserCourseModel> UserCourses
        {
            get
            {
                if (_userCourses == null)
                {
                    _userCourses = Database.GetAll<UserCourseModel>("WHERE UserID = @UserID", new {UserID = ID});
                }
                return _userCourses;
            }
            set { _userCourses = value; }
        }

        private IEnumerable<CourseModel> _directCourses;
        [DBIgnore]
        public IEnumerable<CourseModel> DirectCourses
        {
            get
            {
                if (_directCourses == null)
                {
                    _directCourses = AllCourses().Where(x => !Company.Courses.Select(y => y.CourseID).Contains(x.ID));
                }
                return _directCourses;
            }
            set { _directCourses = value; }
        }

        private string RepresentingCompanyKey => ID + "-representing";

        private CompanyModel _activeCompany;
        /// <summary>
        /// This is the company the user is currently representing
        /// </summary>
        [DBIgnore]
        public CompanyModel RepresentingCompany
        {
            get
            {
                if (_activeCompany == null)
                {
                    var ctx = HttpContext.Current;




                    //if (ctx.Session != null && ctx.Session[RepresentingCompanyKey] != null)
                    //    if (ctx.Session[RepresentingCompanyKey] != null)
                    //    {
                            var repID = (int?)ctx.Session[RepresentingCompanyKey];

                            if (repID == null)
                            {
                                repID = Company.ID;
                                ctx.Session[RepresentingCompanyKey] = repID.Value;
                            }

                            _activeCompany = Database.GetSingle<CompanyModel>(repID.Value);
                    //    }

                }
                return _activeCompany;
            }
            set { _activeCompany = value; }
        }

        public void SetRepresentingCompany(int companyID)
        {
            var ctx = HttpContext.Current;
            ctx.Session[RepresentingCompanyKey] = companyID;
            RepresentingCompany = null;
        }

        public void StartCourse(CourseModel course)
        {
            var matchedCourse = UserCourses.FirstOrDefault(x => x.CourseID == course.ID);
             UsersService _users = new UsersService();
        int CompanyId= _users.GetLoggedInUserModel().Company.ID;



            var curUser = _users.GetLoggedInUserModel();
            var curUserCourse = curUser.UserCourses.Where(x => x.CourseID == course.ID).FirstOrDefault();
            int TrainingCoursesWeeks = Database.Query<int>("select TrainingCoursesWeeks from settings where companyid=" + CompanyId).FirstOrDefault();

            if (matchedCourse == null)
            {
                //Starting from scratch

                Database.Execute(
                    "INSERT INTO UserCourses" +
                    "   (UserID, CourseID, ComplianceScoreMinimum, StartedOn, CourseStageID, RetakeDate) " +
                    "VALUES" +
                    "   (@UserID, @CourseID, @ComplianceScoreMinimum, @StartedOn, @CourseStageID, @RetakeDate)",
                    new
                    {
                        UserID = ID,

                        CourseID = course.ID,
                        ComplianceScoreMinimum =
                            Company.Courses.First(x => x.CourseID == course.ID).ComplianceScoreMinimum,
                        StartedOn = DateTime.Now,
                        CourseStageID = course.FirstStage.ID,
                        RetakeDate = DateTime.Now.AddDays(Convert.ToDouble(course.RetakeDuration))
                    });

                //if (course.RetakeDuration>30)
                //{
                //    int AssignedCourseID = Database.Query<int>("select id from AssignedCourses where companyid=" + CompanyId + "and userid=" + ID + "and courseid=" + course.ID).FirstOrDefault();
                //    Database.ExecuteUpdate("AssignedCourses", new[] { "AssignedCourseDate", "ReminderStatus" }, new { ID = AssignedCourseID, AssignedCourseDate = DateTime.Now.AddDays(Convert.ToDouble(course.RetakeDuration - 30)), ReminderStatus = 99 });

                //}







            }
            else
            {
                if (!matchedCourse.StartedOn.HasValue)
                {
                    matchedCourse.StartedOn = DateTime.Now;
                    matchedCourse.CourseStageID = course.FirstStage.ID;
                    if (!matchedCourse.ComplianceScoreMinimum.HasValue)
                    {
                        matchedCourse.ComplianceScoreMinimum = Company.Courses.First(x => x.CourseID == course.ID).ComplianceScoreMinimum;
                    }

                    Database.ExecuteUpdate(matchedCourse);
                }
            }
        }
    }
}
