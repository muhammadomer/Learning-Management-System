using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using AtlasDB;
using HUC.Web.App.Courses;
using HUC.Web.App.Shared;
using HUC.Web.App.Users.Courses;

namespace HUC.Web.App.Companies.Courses
{
    public class CompanyCourseModel : BaseModel
    {
        public int CompanyID { get; set; }
        public int CourseID { get; set; }
        public int HasFreeTextCheck { get; set; }
        public int ComplianceScoreMinimum { get; set; }
        
        //Lazy
        private CompanyModel _company;
        private CourseModel _course;
        private decimal? _complianceScoreAverage;
        private IEnumerable<UserCourseModel> _userCourses;
        private TimeSpan? _averageTime;
        private string _averageTimeString;

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
        public IEnumerable<UserCourseModel> UserCourses
        {
            get
            {
                if (_userCourses == null)
                {
                    var userIDs = Company.AllUsers.Select(x => x.UserID);
                    if (userIDs.Any())
                    {
                        _userCourses = Database.GetAll<UserCourseModel>("WHERE CourseID = @CourseID AND UserID IN (" + string.Join(", ", userIDs) + ")",
                            new { CourseID = this.CourseID });
                    }
                    else
                    {
                        _userCourses = new List<UserCourseModel>();
                    }
                }
                return _userCourses;
            }
            set { _userCourses = value; }
        }

        [DBIgnore]
        public decimal ComplianceScoreAverage
        {
            get
            {
                if (!_complianceScoreAverage.HasValue)
                {
                    if (this.Course != null)
                    {
                        var curUserCourses = this.UserCourses.Where(x => x.IsComplete);

                        if (!curUserCourses.Any())
                        {
                            _complianceScoreAverage = 0m;
                        }
                        else
                        {
                            var tmpCourseTotals = 0;

                            foreach (var curUserCourse in curUserCourses)
                            {
                                var curCourseTotal = 0;

                                foreach (var curResource in this.Course.Resources)
                                {
                                    var curTest = curResource.LatestUserTestFor(curUserCourse.ID);

                                    if (curTest != null)
                                    {
                                        curCourseTotal += curTest.CorrectAnswerCount;
                                    }
                                }

                                tmpCourseTotals += curCourseTotal;
                            }

                            _complianceScoreAverage = Math.Round((decimal) tmpCourseTotals/curUserCourses.Count(), 2);
                        }
                    }
                    else
                    {
                        _complianceScoreAverage = 0m;
                    }
                }
                return _complianceScoreAverage.Value;
            }
            set { _complianceScoreAverage = value; }
        }

        [DBIgnore]
        public TimeSpan AverageTime
        {
            get
            {
                if (!_averageTime.HasValue)
                {
                    var userCourses = this.UserCourses;

                    if (userCourses.Any())
                    {
                        _averageTime = new TimeSpan((userCourses.Sum(x => x.TimeTaken.Ticks) / userCourses.Count()));
                    }
                    else
                    {
                        _averageTime = new TimeSpan(0);
                    }
                }
                return _averageTime.Value;
            }
            set { _averageTime = value; }
        }

        [DBIgnore]
        public string AverageTimeString
        {
            get
            {
                if (_averageTimeString == null)
                {
                    var tmpStr = "";

                    if (this.AverageTime.TotalHours >= 1)
                    {
                        tmpStr += Math.Floor(this.AverageTime.TotalHours) + " hour" + (Math.Floor(this.AverageTime.TotalHours) > 1 ? "s" : "");
                    }
                    if (this.AverageTime.Minutes > 0)
                    {
                        tmpStr += " " + this.AverageTime.Minutes + " minute" + (this.AverageTime.Minutes > 1 ? "s" : "");
                    }
                    if (this.AverageTime.Seconds > 0)
                    {
                        tmpStr += " " + this.AverageTime.Seconds + " second" + (this.AverageTime.Seconds > 1 ? "s" : "");
                    }

                    _averageTimeString = tmpStr;
                }
                return _averageTimeString;
            }
            set { _averageTimeString = value; }
        }
    }
}
