using System;
using System.Linq;
using AtlasDB;
using HUC.Web.App.Companies;
using HUC.Web.App.Companies.Courses;
using HUC.Web.App.Companies.Users;
using HUC.Web.App.Courses;
using HUC.Web.App.Courses.Stages;
using HUC.Web.App.Courses.Time;
using HUC.Web.App.Heartbeats;
using HUC.Web.App.Shared;

namespace HUC.Web.App.Users.Courses
{
    public class UserCourseModel : BaseModel
    {
        public int UserID { get; set; }
        public int CourseID { get; set; }
        public int? ComplianceScoreMinimum { get; set; }
        public int? CourseStageID { get; set; }
        public DateTime? StartedOn { get; set; }
        public bool IsComplete { get; set; }
        public DateTime? CompleteOn { get; set; }
        public int? ColleagueId { get; set; }
        public int? ClientId { get; set; }
        public int? WorkId { get; set; }
        public DateTime? RetakeDate { get; set; }
        public DateTime? CoolDownHoursTime { get; set; }
        public int? FeedBack { get; set; }
        //Lazy
        private CourseModel _course;
        private UserModel _user;
        private CourseStageModel _courseStage;
        private TimeSpan? _timeTaken;
        private int? _totalScore;
       // private int? _totalAttemptedQuestion;
        private string _timeTakenString;
        private CompanyUserModel _companyUser;
        [DBIgnore]
        public int TotalAttemtedQuestion { get; set; }
        [DBIgnore]
        public CourseModel Course
        {
            get
            {
                if (_course == null)
                {
                    _course = Database.GetSingle<CourseModel>(CourseID);
                }
                return _course;
            }
            set { _course = value; }
        }

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
        public CompanyUserModel CompanyUser
        {
            get
            {
                if (_companyUser == null)
                {
                    var userslist = User.Company.AllUsers.ToList();
                    var getuser = userslist.Where(x => x.UserID == User.ID && x.IsAdmin == false && x.IsBackupAdmin == false);
                    _companyUser = getuser.Single(x => x.UserID == this.UserID);
                }
                return _companyUser;
            }
            set { _companyUser = value; }
        }

        [DBIgnore]
        public CourseStageModel CourseStage
        {
            get
            {
                if (_courseStage == null)
                {
                    _courseStage = Database.GetSingle<CourseStageModel>(this.CourseStageID);
                }
                return _courseStage;
            }
            set { _courseStage = value; }
        }

        [DBIgnore]
        public TimeSpan TimeTaken
        {
            get
            {
                if (!_timeTaken.HasValue)
                {
                    var time = Database.GetAll<CourseTimeModel>("WHERE UserCourseID = @ID", new { ID = this.ID });

                    _timeTaken = new TimeSpan(time.Sum(x => (x.EndOn - x.StartOn).Ticks));

                    
                       var now = DateTime.Now.AddSeconds(-10);

                    //  var time = DateTime.Now.AddSeconds(-10);
                    var connections =
                         Database.Query<HeartbeatModel>("SELECT * FROM Heartbeats WHERE EndOn is null AND LastBeatOn < @currentTime and UserCourseID = @ID",
                        //Database.Query<HeartbeatModel>("SELECT * FROM Heartbeats WHERE EndOn is null AND  UserCourseID = @ID order by ID Desc",
                             //new { ID = this.ID }).FirstOrDefault();
                             new { currentTime = now, ID = this.ID }).FirstOrDefault();
                    
                        //Add the time spent...
                        if(connections != null)
                        {
                            DateTime dt = connections.LastBeatOn ?? DateTime.Now;
                       
                         _timeTaken.Value.Add(new TimeSpan((dt - connections.StartOn).Ticks));
                    }                        
                    


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
        public int TotalScore
        {
            get
            {
                if (!_totalScore.HasValue)
                {
                    var curTotal = 0;
                    this.TotalAttemtedQuestion = curTotal;
                    foreach (var curResource in this.Course.Resources)
                    {
                        var curTest = curResource.LatestUserTestFor(this.ID);
                        

                        if (curTest != null)
                        {
                            this.TotalAttemtedQuestion += curTest.UserQuestions.Count();
                            curTotal += curTest.CorrectAnswerCount;

                         
                           
                        }
                        //this.TotalAttemtedQuestion++;
                    }

                    _totalScore = curTotal;
                   
                }
                return _totalScore.Value;
            }
            set { _totalScore = value; }
        } 
        //Question Count that are Correct or Wrong but Attempted by the user
        //[DBIgnore]
        //public int TotalAttemtedQuestion
        //{
        //    get
        //    {
        //        if (!_totalAttemptedQuestion.HasValue)
        //        {
        //            var curTotal = 0;

        //            foreach (var curResource in this.Course.Resources)
        //            {
        //                var curTest = curResource.LatestUserTestFor(this.ID);

        //                if (curTest != null)
        //                {
        //                    curTotal++;
        //                }
        //            }

        //            _totalAttemptedQuestion = curTotal;
        //        }
        //        return _totalAttemptedQuestion.Value;
        //    }
        //    set { _totalAttemptedQuestion = value; }
        //}

        [DBIgnore]
        public bool IsPass
        {
            get { return TotalScore >= ComplianceScoreMinimum; }
        }
    }
}