using System.Linq;
using AtlasDB;
using HUC.Web.App.Resources;
using HUC.Web.App.Resources.Chapters;
using HUC.Web.App.Resources.Questions;
using HUC.Web.App.Shared;
using HUC.Web.App.Users;
using HUC.Web.App.Users.Courses;

namespace HUC.Web.App.Courses.Stages
{
    public class CourseStageModel : BaseModel
    {
        public int CourseID { get; set; }
        public int Stage { get; set; }

        public int? ResourceID { get; set; }
        public int? ChapterID { get; set; }
        public int? TestQuestionID { get; set; }

        public bool IsStart { get; set; }
        public bool IsTestStart { get; set; }
        public bool IsTest { get; set; }
        public bool IsTestEnd { get; set; }
        public bool IsEnd { get; set; }

        //Lazy
        private CourseStageModel _nextStage;
        private CourseStageModel _prevStage;
        private ResourceModel _resource;
        private ResourceChapterModel _chapter;
        private TestQuestionModel _testQuestion;

        [DBIgnore]
        public CourseStageModel NextStage
        {
            get
            {
                if (_nextStage == null)
                {
                    _nextStage = Database.GetAll<CourseStageModel>("WHERE CourseID = @CourseID AND Stage = @Stage", new { CourseID = this.CourseID, Stage = (this.Stage + 1) }).SingleOrDefault();
                }
                return _nextStage;
            }
            set { _nextStage = value; }
        }

        [DBIgnore]
        public CourseStageModel PrevStage
        {
            get
            {
                if (_prevStage == null)
                {
                    _prevStage = Database.GetAll<CourseStageModel>("WHERE CourseID = @CourseID AND Stage = @Stage", new { CourseID = this.CourseID, Stage = (this.Stage - 1) }).SingleOrDefault();
                }
                return _prevStage;
            }
            set { _prevStage = value; }
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

        public ResourceChapterModel Chapter
        {
            get
            {
                if (_chapter == null)
                {
                    _chapter = Database.GetSingle<ResourceChapterModel>(this.ChapterID);
                }
                return _chapter;
            }
            set { _chapter = value; }
        }

        public TestQuestionModel TestQuestion
        {
            get
            {
                if (_testQuestion == null)
                {
                    _testQuestion = Database.GetSingle<TestQuestionModel>(this.TestQuestionID);
                }
                return _testQuestion;
            }
            set { _testQuestion = value; }
        }

        public bool CanBeSkippedTo(int userCourseID)
        {
            var userCourse = Database.GetSingle<UserCourseModel>(userCourseID);
            //Return false if the stage ID is the current stage ID
            if (userCourse.CourseStageID == this.ID)
            {
                return false;
            }

            UserCourseTestModel userCourseTest = null;
            if (userCourse.CourseStage.Resource != null)
            {
                userCourseTest = userCourse.CourseStage.Resource.LatestUserTestFor(userCourse.ID);
            }
            //Test Checks!
            //If the user is currently in a test, they can skip to any part of that test (But ONLY to another part of the test, everywhere else should be blocked including chapters etc)
            if (!userCourse.CourseStage.IsTest || userCourse.CourseStage.IsTestStart || (userCourse.CourseStage.IsTestEnd && userCourseTest != null && userCourseTest.IsComplete))
            {
                //If a user is not in a test, you cannot skip to any test question or test end. Normal skip checking proceeds
                if (!this.IsTest || this.IsTestStart || (this.IsTestEnd && userCourseTest != null && userCourseTest.IsComplete))
                {
                    var stagesLeadingToDestinationStage =
                        Database.GetAll<CourseStageModel>("WHERE CourseID = @CourseID AND Stage < @DestStage",
                            new {CourseID = this.CourseID, DestStage = this.Stage});

                    //Check all 'Test Start' items, check if the user has completed a test
                    foreach (var curTestStart in stagesLeadingToDestinationStage.Where(x => x.IsTestStart))
                    {
                        var curUserTest = curTestStart.Resource.LatestUserTestFor(userCourseID);
                        if (curUserTest == null || !curUserTest.IsComplete)
                        {
                            return false;
                        }
                    }

                    return true;
                }

                //Need to check if the user has completed all tests before this stage
                return false;
            }
            else
            {
                if (this.IsTestStart || (this.TestQuestion == null && !this.IsTest))
                {
                    return false;
                }
                else
                {
                    return userCourse.CourseStage.Resource.ID == this.Resource.ID;
                }
            }
        }
    }
}
