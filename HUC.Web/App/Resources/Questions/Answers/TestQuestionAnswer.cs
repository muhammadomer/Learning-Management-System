using System.ComponentModel.DataAnnotations;
using AtlasDB;
using HUC.Web.App.Shared;

namespace HUC.Web.App.Resources.Questions.Answers
{
    public class TestQuestionAnswerModel : BaseModel
    {
        public int TestQuestionID { get; set; }
        public int Sort { get; set; }

        [Required, Display(Name = "Answer")]
        public string Answer { get; set; }

        [Required, Display(Name = "Is This A Correct Answer?")]
        public bool IsCorrect { get; set; }

        public int CourseUserId { get; set; }

        public int IsResult { get; set; }

        //Lazy
        private TestQuestionModel _question;

        [DBIgnore]
        public TestQuestionModel Question
        {
            get
            {
                if (_question == null)
                {
                    _question = Database.GetSingle<TestQuestionModel>(this.TestQuestionID);
                }
                return _question;
            }
            set { _question = value; }
        }
    }
    public class TestQuestionAnswerModelForJson
    {
        public string answerid { get; set; }
        public string option { get; set; }
        public string type { get; set; }
        public string selected { get; set; }
        public string questionid { get; set; }
    }
}