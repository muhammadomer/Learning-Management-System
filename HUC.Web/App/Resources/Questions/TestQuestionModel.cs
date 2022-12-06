using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using AtlasDB;
using HUC.Web.App.Resources.Questions.Answers;
using HUC.Web.App.Shared;

namespace HUC.Web.App.Resources.Questions
{
    public class TestQuestionModel : BaseModel
    {
        public int Sort { get; set; }
        public int ResourceID { get; set; }
        [Required, Display(Name = "Attempts (Minimum 1)"),Range(1,10)]
        public int? Attempts { get; set; }

        [Required, Display(Name = "Question")]
        public string Question { get; set; }
        [Required, Display(Name = "Question Type")]
        public int? QuestionType { get; set; }
        [DBIgnore]
        public List<int> AnswerId { get; set; }
        [DBIgnore]
        public int AnswerOrder { get; set; }
       // [DBIgnore]
       // public int? Attempts { get; set; }

        [Display(Name = "Feedback")]
        public string Feedback { get; set; }

        //Lazy
        private IEnumerable<TestQuestionAnswerModel> _answers;
        private ResourceModel _resource;

        [DBIgnore]
        public IEnumerable<TestQuestionAnswerModel> Answers
        {
            get
            {
                if (_answers == null)
                {
                    _answers = Database.GetAll<TestQuestionAnswerModel>("WHERE TestQuestionID = @ID order by Sort", new {ID = this.ID});
                }
                return _answers;
            }
            set { _answers = value; }
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
    }
}