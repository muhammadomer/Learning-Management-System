using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AtlasDB;
using HUC.Web.App.Companies.Courses;
using HUC.Web.App.Companies.Users;
using HUC.Web.App.Courses;
using HUC.Web.App.Shared;
using HUC.Web.App.Users;
using HUC.Web.App.Users.Courses;

namespace HUC.Web.App.Companies
{
    public class CompanyModel : BaseModel
    {
        [Required, StringLength(500), Display(Name = "Name")]
        public string Name { get; set; }

        [Required, Display(Name = "User Limit")]
        public int UserLimit { get; set; }

        [Required, Display(Name = "Is Demonstration Company?")]
        public bool IsDemonstration { get; set; }

        [Required, Display(Name = "Is Initial Verification Email Enabled?")]
        public bool IsInitialVerificationEnabled { get; set; }

        [DBIgnore, Display(Name = "Company Courses")]
        public IEnumerable<int> CourseIDs { get; set; }

        public int? ParentCompanyID { get; set; }

        public List<SelectListItem> CourseOptions(int CompanyId = 0)
        {
            var sli = new List<SelectListItem>();

            IEnumerable<CourseModel> courses;
            if (CompanyId > 0)
            {
                courses = Database.GetAll<CourseModel>("WHERE IsDeleted = 0 AND IsPublished = 1 AND ID IN ( SELECT CourseID FROM CompanyCourses WHERE CompanyID = " + CompanyId + ")");
            }
            else
            {
                if (this.ParentCompanyID.HasValue)
                {
                    courses = this.ParentCompany.Courses.Select(x => x.Course);
                }
                else
                {
                    courses = Database.GetAll<CourseModel>("WHERE IsDeleted = 0 AND IsPublished = 1 ");
                }
            }
            foreach (var curCourse in courses)
            {
                sli.Add(new SelectListItem
                {
                    Text = curCourse.Name,
                    Value = curCourse.ID.ToString()
                });
            }

            return sli;
        }
        public List<SelectListItem> CourseadminOptions(int CompanyId = 0)
        {
            var sli = new List<SelectListItem>();

            IEnumerable<CourseModel> courses;
            if (CompanyId > 0)
            {
                courses = Database.GetAll<CourseModel>("WHERE IsDeleted = 0 AND IsPublished = 1 AND IsCreatedBy = 0 AND ID IN ( SELECT CourseID FROM CompanyCourses WHERE CompanyID = " + CompanyId + ")");
            }
            else
            {
                if (this.ParentCompanyID.HasValue)
                {
                    courses = this.ParentCompany.Courses.Select(x => x.Course);
                }
                else
                {
                    courses = Database.GetAll<CourseModel>("WHERE IsDeleted = 0 AND IsPublished = 1 AND IsCreatedBy = 0");
                }
            }
            foreach (var curCourse in courses)
            {
                sli.Add(new SelectListItem
                {
                    Text = curCourse.Name,
                    Value = curCourse.ID.ToString()
                });
            }

            return sli;
        }

        //Lazy
        private IEnumerable<CompanyCourseModel> _courses;
        private IEnumerable<CompanyUserModel> _users;
        private int? _usersCount;
        private IEnumerable<UserCourseTestModel> _latestUserTests;

        [DBIgnore]
        public IEnumerable<CompanyCourseModel> Courses
        {
            get
            {
                if (_courses == null)
                {
                    _courses = Database.GetAll<CompanyCourseModel>("WHERE CompanyID = @ID ", new { ID = this.ID });
                }
                return _courses;
            }
            set { _courses = value; }
        }

        [DBIgnore]
        public IEnumerable<CompanyUserModel> AllUsers
        {
            get
            {
                if (_users == null)
                {
                    _users = Database.GetAll<CompanyUserModel>("WHERE CompanyID = @ID  AND IsDeleted = 0", new { ID = this.ID });
                }
                return _users;
            }
            set { _users = value; }
        }

        [DBIgnore]
        public int ThisCompanyUserCount
        {
            get
            {
                if (_usersCount == null)
                {
                    _usersCount = this.AllUsers.Count() - this.AllUsers.Count(x => x.IsAdmin);
                }
                return _usersCount.Value;
            }
            set { _usersCount = value; }
        }

        [DBIgnore]
        public IEnumerable<UserCourseTestModel> LatestUserTests(int? year = null)
        {

            if (_latestUserTests == null)
            {
                var query = "";
                object parameters;
                if (year.HasValue)
                {
                    parameters = new { companyID = ID, year };
                    query =
                            "WHERE UserCourseID IN (SELECT ID FROM UserCourses WHERE UserID IN (SELECT UserID FROM CompanyUsers WHERE CompanyID = @CompanyID)) AND year(StartOn) = @year";
                }
                else
                {
                    parameters = new { companyID = ID };
                    query =
                        "WHERE UserCourseID IN (SELECT ID FROM UserCourses WHERE UserID IN (SELECT UserID FROM CompanyUsers WHERE CompanyID = @CompanyID))";
                }

                var allUserTestsGrouped = Database.GetAll<UserCourseTestModel>(query, parameters).GroupBy(x => new { x.UserCourseID, x.ResourceID });

                var retList = new List<UserCourseTestModel>();

                foreach (var curTestGroup in allUserTestsGrouped)
                {
                    retList.Add(curTestGroup.OrderBy(x => x.StartOn).LastOrDefault());
                }

                _latestUserTests = retList.Where(x => x.IsComplete);
            }
            return _latestUserTests;

        }

        private int? _usersUsed;
        [DBIgnore]
        public int UsersUsed
        {
            get
            {
                if (_usersUsed == null)
                {
                    //The users used for this company is the total of all users within itself and all children.
                    var rawUsers = this.AllDescendantUsersIncludingSelf.Count(x => !x.IsAdmin && !x.IsBackupAdmin);

                    var backupUsersCourseUsable = this.AllDescendantUsersIncludingSelf.Count(x => x.IsBackupAdmin && x.IsBackupAdminCourseUsable);

                    _usersUsed = rawUsers + backupUsersCourseUsable;
                }
                return _usersUsed.Value;
            }
            set { _usersUsed = value; }
        }

        private int? _usersAvailable;
        [DBIgnore]
        public int UsersAvailable
        {
            get
            {
                if (_usersAvailable == null)
                {
                    //First we need to get how many users are remaining for the top parent company
                    var topLevelUsersRemaining = this.TopCompany.UserLimit - this.TopCompany.UsersUsed;

                    //If the top parent users remaining is less than the current company user limit, use that as our limit otherwise use the company user limit.

                    if (topLevelUsersRemaining < this.UserLimit)
                    {
                        _usersAvailable = topLevelUsersRemaining;
                    }
                    else
                    {
                        _usersAvailable = this.UserLimit;
                    }
                }
                return _usersAvailable.Value;
            }
            set { _usersAvailable = value; }
        }

        private IEnumerable<CompanyUserModel> _allDescendantUsersIncludingSelf;
        [DBIgnore]
        public IEnumerable<CompanyUserModel> AllDescendantUsersIncludingSelf
        {
            get
            {
                if (_allDescendantUsersIncludingSelf == null)
                {
                    if (this.DescendantIDsIncludingSelf.Any())
                    {
                        _allDescendantUsersIncludingSelf = Database.GetAll<CompanyUserModel>("WHERE IsDeleted = 0 AND CompanyID IN (" + String.Join(", ", this.DescendantIDsIncludingSelf) + ")");
                    }
                    else
                    {

                        _allDescendantUsersIncludingSelf = new List<CompanyUserModel>();
                    }
                }
                return _allDescendantUsersIncludingSelf;
            }
            set { _allDescendantUsersIncludingSelf = value; }
        }

        private CompanyModel _topCompany;
        [DBIgnore]
        public CompanyModel TopCompany
        {
            get
            {
                if (_topCompany == null)
                {
                    if (this.TopCompanyID.HasValue)
                    {
                        _topCompany = Database.GetSingle<CompanyModel>(this.TopCompanyID);
                    }
                    else
                    {
                        return this;
                    }
                }
                return _topCompany;
            }
            set { _topCompany = value; }
        }

        private int? _topCompanyID;
        [DBIgnore]
        public int? TopCompanyID
        {
            get
            {
                if (_topCompanyID == null)
                {
                    var id = Database.Query<int?>(
                        "WITH RCTE AS " +
                        "( " +
                        "   SELECT *, 1 AS Lvl FROM Companies " +
                        "   WHERE ID = @childID " +
                        "   UNION ALL " +
                        "   SELECT rh.*, Lvl+1 AS Lvl FROM dbo.Companies rh " +
                        "   INNER JOIN RCTE rc ON rh.ID = rc.ParentCompanyID " +
                        ") " +
                        "SELECT TOP 1 r.ParentCompanyID " +
                        "FROM RCTE r " +
                        "INNER JOIN dbo.Companies c ON c.id = r.ParentCompanyID " +
                        "ORDER BY lvl DESC", new { childID = ID }).FirstOrDefault();

                    _topCompanyID = id;
                }
                return _topCompanyID;
            }
            set { _topCompanyID = value; }
        }

        private IEnumerable<int> _descendantIDsIncludingSelf;
        [DBIgnore]
        public IEnumerable<int> DescendantIDsIncludingSelf
        {
            get
            {
                if (_descendantIDsIncludingSelf == null)
                {
                    _descendantIDsIncludingSelf = Database.Query<int>(
                        "WITH cte AS " +
                        "(" +
                        "   SELECT a.Id, a.ParentCompanyID " +
                        "   FROM Companies a " +
                        "   WHERE Id = @Id " +
                        "   UNION ALL " +
                        "   SELECT a.Id, a.ParentCompanyID " +
                        "   FROM Companies a JOIN cte c ON a.ParentCompanyID = c.id " +
                        ")" +
                        "SELECT ID FROM cte",
                        new { Id = this.ID });
                }
                return _descendantIDsIncludingSelf;
            }
            set { _descendantIDsIncludingSelf = value; }
        }

        private IEnumerable<CompanyModel> _subCompanies;
        [DBIgnore]
        public IEnumerable<CompanyModel> SubCompanies
        {
            get
            {
                if (_subCompanies == null)
                {
                    _subCompanies = Database.GetAll<CompanyModel>("WHERE ParentCompanyID = @ID", new { ID = this.ID });
                }
                return _subCompanies;
            }
            set { _subCompanies = value; }
        }

        private IEnumerable<CompanyModel> _descendantCompaniesIncSelf;
        [DBIgnore]
        public IEnumerable<CompanyModel> DescendantCompaniesIncSelf
        {
            get
            {
                if (_descendantCompaniesIncSelf == null)
                {
                    var ret = new List<CompanyModel>();
                    ret.Add(this);

                    if (SubCompanies.Any())
                    {
                        ret.AddRange(SubCompanies.SelectMany(x => x.DescendantCompaniesIncSelf));
                    }
                    _descendantCompaniesIncSelf = ret;
                }
                return _descendantCompaniesIncSelf;
            }
            set { _descendantCompaniesIncSelf = value; }
        }

        private CompanyModel _parentCompany;
        [DBIgnore]
        public CompanyModel ParentCompany
        {
            get
            {
                if (_parentCompany == null)
                {
                    _parentCompany = Database.GetSingle<CompanyModel>(this.ParentCompanyID);
                }
                return _parentCompany;
            }
            set { _parentCompany = value; }
        }

        private List<CompanyModel> _companyTrace;
        [DBIgnore]
        public List<CompanyModel> CompanyTrace
        {
            get
            {
                if (_companyTrace == null)
                {
                    _companyTrace = new List<CompanyModel>();

                    if (this.ParentCompanyID.HasValue && this.ParentCompany != null)
                    {
                        _companyTrace.AddRange(this.ParentCompany.CompanyTrace);
                    }
                    _companyTrace.Add(this);
                }
                return _companyTrace;
            }
            set { _companyTrace = value; }
        }
    }
}