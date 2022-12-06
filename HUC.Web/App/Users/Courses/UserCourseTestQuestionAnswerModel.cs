using AtlasDB;
using HUC.Web.App.Resources.Questions.Answers;
using HUC.Web.App.Shared;

namespace HUC.Web.App.Users.Courses
{
    public class UserCourseTestQuestionAnswerModel : BaseModel
    {
        public int UserCourseQuestionID { get; set; }
        public int TestAnswerID { get; set; }

        //Lazy
        private TestQuestionAnswerModel _testAnswer;

        [DBIgnore]
        public TestQuestionAnswerModel TestAnswer
        {
            get
            {
                if (_testAnswer == null)
                {
                    _testAnswer = Database.GetSingle<TestQuestionAnswerModel>(this.TestAnswerID);
                }
                return _testAnswer;
            }
            set { _testAnswer = value; }
        }
    }
}