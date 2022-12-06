using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AtlasDB;
using HUC.Web.App.Courses.Time;
using HUC.Web.App.Resources;
using HUC.Web.App.Resources.Questions;
using HUC.Web.App.Shared;

namespace HUC.Web.App.Users.Courses
{
    public class UserCourseTestModel : BaseModel
    {
        public int UserCourseID { get; set; }
        public int ResourceID { get; set; }

        public DateTime StartOn { get; set; }
        public bool IsComplete { get; set; }
        public DateTime? CompleteOn { get; set; }

        //Lazy
        private IEnumerable<UserCourseTestQuestionModel> _userQuestions;
        private IEnumerable<TestQuestionModel> _testQuestions;
        private int? _correctAnswerCount;
        private ResourceModel _resource;
        private UserCourseModel _userCourse;
        private TimeSpan? _timeTaken;
        private string _timeTakenString;
        private int? _maxScore;

        [DBIgnore]
        public IEnumerable<UserCourseTestQuestionModel> UserQuestions
        {
            get
            {
                if (_userQuestions == null)
                {
                    _userQuestions = Database.GetAll<UserCourseTestQuestionModel>("WHERE UserCourseTestID = @ID",
                        new { ID = this.ID });
                }
                return _userQuestions;
            }
            set { _userQuestions = value; }
        }

        [DBIgnore]
        public IEnumerable<TestQuestionModel> TestQuestions
        {
            get
            {
                if (_testQuestions == null)
                {
                    _testQuestions = Database.GetAll<TestQuestionModel>("WHERE ResourceID = @ResourceID", new { ResourceID = this.ResourceID });
                }
                return _testQuestions;
            }
            set { _testQuestions = value; }
        }

        [DBIgnore]
        public int CorrectAnswerCount
        {
            get
            {
                if (!_correctAnswerCount.HasValue)
                {
                    _correctAnswerCount = this.UserQuestions.Count(x => x.IsCorrect);
                }
                return _correctAnswerCount.Value;
            }
            set { _correctAnswerCount = value; }
        }

        [DBIgnore]
        public ResourceModel Resource
        {
            get
            {
                if (_resource == null)
                {
                    _resource = Database.GetSingle<ResourceModel>(this.ResourceID);
                }
                return _resource;
            }
            set { _resource = value; }
        }

        [DBIgnore]
        public UserCourseModel UserCourse
        {
            get
            {
                if (_userCourse == null)
                {
                    _userCourse = Database.GetSingle<UserCourseModel>(this.UserCourseID);
                }
                return _userCourse;
            }
            set { _userCourse = value; }
        }

        [DBIgnore]
        public TimeSpan TimeTaken
        {
            get
            {
                if (!_timeTaken.HasValue)
                {
                    var times = Database.GetAll<CourseTimeModel>("WHERE UserCourseTestID = @ID", new { this.ID });

                    _timeTaken = new TimeSpan(times.Sum(x => (x.EndOn - x.StartOn).Ticks));
                }
                return _timeTaken.Value;
            }
            set { _timeTaken = value; }
        }

        [DBIgnore]
        public string TimeTakenString
        {
            get
            {
                if (_timeTakenString == null)
                {
                    var tmpStr = "";

                    if (this.TimeTaken.TotalHours >= 1)
                    {
                        tmpStr += Math.Floor(this.TimeTaken.TotalHours) + " hour" + (Math.Floor(this.TimeTaken.TotalHours) > 1 ? "s" : "");
                    }
                    if (this.TimeTaken.Minutes > 0)
                    {
                        tmpStr += " " + this.TimeTaken.Minutes + " minute" + (this.TimeTaken.Minutes > 1 ? "s" : "");
                    }
                    if (this.TimeTaken.Seconds > 0)
                    {
                        tmpStr += " " + this.TimeTaken.Seconds + " second" + (this.TimeTaken.Seconds > 1 ? "s" : "");
                    }

                    _timeTakenString = tmpStr;
                }
                return _timeTakenString;
            }
            set { _timeTakenString = value; }
        }

        [DBIgnore]
        public int MaxScore
        {
            get
            {
                if (!_maxScore.HasValue)
                {
                    _maxScore = this.TestQuestions.Count();
                }
                return _maxScore.Value;
            }
            set { _maxScore = value; }
        }
    }
}