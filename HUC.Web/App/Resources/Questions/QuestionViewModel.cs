using AtlasDB;
using HUC.Web.App.Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HUC.Web.App.Resources.Questions
{
    public class QuestionViewModel:BaseModel
    {
            public int TestQuestionID { get; set; }
            public int Sort { get; set; }

            [Required, Display(Name = "Answer")]
            public string Answer { get; set; }

            [Required, Display(Name = "Is This A Correct Answer?")]
            public bool IsCorrect { get; set; }

            public int CourseUserId { get; set; }

            public int IsResult { get; set; }

            public string ModuleName { get; set; }
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
}