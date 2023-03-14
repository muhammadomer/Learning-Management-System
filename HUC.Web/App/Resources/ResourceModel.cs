using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;
using AtlasDB;
using HUC.Web.App.Companies;
using HUC.Web.App.Companies.Courses;
using HUC.Web.App.Courses;
using HUC.Web.App.Courses.Time;
using HUC.Web.App.Resources.Chapters;
using HUC.Web.App.Resources.Questions;
using HUC.Web.App.Shared;
using HUC.Web.App.Users.Courses;

namespace HUC.Web.App.Resources
{
    public class ResourceModel : BaseModel
    {
        public int Sort { get; set; }
        public int CourseID { get; set; }
        [DBIgnore]
        public int TotalResource { get; set; }
        [DBIgnore]
        public int Percentage { get; set; }
        [DBIgnore]
        public bool ModuleCompleted { get; set; }

        [Required, StringLength(500), Display(Name = "Name")]
        public string Name { get; set; }

        [Display(Name = "Test Cooldown (Hours)")]
        public decimal? TestCooldownHours { get; set; }

        [Display(Name = "Test Time Limit (Minutes)")]
        public int? TestTimeLimitMinutes { get; set; }
        [RegularExpression("^[0-9]*$") ,Required(ErrorMessage ="Must be a numeric value and max range is 1-200 minutes"),Range(1,200), Display(Name = "Module Time Limit (Max 200 Minutes)")]
        public int? ModuleTime { get; set; }
        [Display(Name = "Module Intro Text"), AllowHtml]
        public string TestIntroCopy { get; set; }
        [Display(Name = "Test Outro Text"), AllowHtml]
        public string TestOutroCopy { get; set; }

        [DBIgnore]
        public decimal AverageRevisionTimeMinutes(CompanyModel company)
        {
            IEnumerable<CourseTimeModel> timings = new List<CourseTimeModel>();

            timings = Database.GetAll<CourseTimeModel>(
                "WHERE UserCourseID IN (SELECT ID FROM UserCourses WHERE CourseID = @CourseID AND UserID IN (SELECT UserID FROM CompanyUsers WHERE CompanyID = @CompanyID)) AND ChapterID IN (SELECT ID FROM ResourceChapters WHERE ResourceID = @ID)",
                new { CourseID = CourseID, ID = this.ID, CompanyID = company.ID });
            if(timings.Count() > 0)
            {

            return timings.GroupBy(x => x.UserCourseID).Average(x => x.Sum(y => y.TimeMinutesCalculated));
            }
            else
            {
                return 0;
            }

//            return timings.Average(x => x.TimeMinutesCalculated);
        }

        //Lazy
        private IEnumerable<ResourceChapterModel> _chapters;
        private CourseModel _course;
        private IEnumerable<TestQuestionModel> _questions;
        private ResourceModel _nextResource;
        private ResourceModel _prevResource;

        [DBIgnore]
        public IEnumerable<ResourceChapterModel> Chapters
        {
            get
            {
                if (_chapters == null)
                {
                    _chapters = Database.GetAll<ResourceChapterModel>("WHERE ResourceID = @ID", new { ID = this.ID });
                }
                return _chapters;
            }
            set { _chapters = value; }
        }

        [DBIgnore]
        public CourseModel Course
        {
            get
            {
                if (_course == null)
                {
                    _course = Database.GetSingle<CourseModel>(this.CourseID);
                }
                return _course;
            }
            set { _course = value; }
        }

        [DBIgnore]
        public IEnumerable<TestQuestionModel> Questions
        {
            get
            {
                if (_questions == null)
                {
                    _questions = Database.GetAll<TestQuestionModel>("WHERE ResourceID = @ID", new { ID = this.ID });
                }
                return _questions;
            }
            set { _questions = value; }
        }

        [DBIgnore]
        public ResourceModel NextResource
        {
            get
            {
                if (_nextResource == null)
                {
                    _nextResource = Database.GetAll<ResourceModel>("WHERE CourseID = @CourseID AND Sort = @Sort", new { CourseID = this.CourseID, Sort = (this.Sort + 1) }).SingleOrDefault();
                }
                return _nextResource;
            }
            set { _nextResource = value; }
        }

        [DBIgnore]
        public ResourceModel PrevResource
        {
            get
            {
                if (_prevResource == null)
                {
                    _prevResource = Database.GetAll<ResourceModel>("WHERE CourseID = @CourseID AND Sort = @Sort", new { CourseID = this.CourseID, Sort = (this.Sort - 1) }).SingleOrDefault();
                }
                return _prevResource;
            }
            set { _prevResource = value; }
        }

        [DBIgnore]
        public UserCourseTestModel LatestUserTestFor(int userCourseID)
        {
            var tests = this.UserTestsFor(userCourseID);

            if (tests.Any())
            {
                return tests.OrderByDescending(x => x.StartOn).First();
            }
            else
            {
                return null;
            }
        }

        [DBIgnore]
        public IEnumerable<UserCourseTestModel> UserTestsFor(int userCourseID)
        {
            var model = Database.GetAll<UserCourseTestModel>("WHERE UserCourseID = @UserCourseID AND ResourceID = @ID", new { UserCourseID = userCourseID, ID = this.ID });

            return model;
        }

        [DBIgnore]
        public IEnumerable<UserCourseTestModel> LatestUserTestsForCompany(int companyID,int? year=null)
        {
            var tmpList = new List<UserCourseTestModel>();

            foreach (var curUserTests in this.UserTestsForCompany(companyID,year).GroupBy(x => x.UserCourse.UserID))
            {
                //if (year == null)
                //{
                    tmpList.Add(curUserTests.OrderByDescending(x => x.StartOn).First());

                //}
                //else
                //{
                //    tmpList.Add(curUserTests.Where(x => Convert.ToDateTime(x.CompleteOn).Year == year).OrderByDescending(x => x.StartOn).First());

                //}
            }

            return tmpList;
        }

        [DBIgnore]
        public IEnumerable<UserCourseTestModel> UserTestsForCompany(int companyID,int? year=null)
        {

            string yearStr = "";
            if (year != null)
            {
                yearStr = "year(starton)="+year+ " and ";
            }

            var model = Database.GetAll<UserCourseTestModel>(
                "WHERE ResourceID = @ID AND UserCourseID IN (SELECT ID FROM UserCourses WHERE "+yearStr+" UserID IN (SELECT UserID FROM CompanyUsers WHERE CompanyID = @CompanyID))", 
                new { ID = this.ID, CompanyID = companyID });

            return model;
        }

        [DBIgnore]
        public DateTime? TestRetakeDateFor(int userCourseID)
        {
            if (this.TestCooldownHours == null)
            {
                return null;
            }
            else
            {
                var lastTest = this.LatestUserTestFor(userCourseID);
                if (lastTest == null)
                {
                    return DateTime.Now;
                }
                else
                {
                    var offsetMinutes = this.TestTimeLimitMinutes.HasValue ? this.TestTimeLimitMinutes.Value : 0;
                    return lastTest.StartOn.AddHours((double) this.TestCooldownHours.Value).AddMinutes(offsetMinutes);
                }
            }
        }
    }
}