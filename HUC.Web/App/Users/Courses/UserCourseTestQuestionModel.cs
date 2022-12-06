using System;
using System.Collections.Generic;
using System.Linq;
using AtlasDB;
using HUC.Web.App.Resources.Questions;
using HUC.Web.App.Resources.Questions.Answers;
using HUC.Web.App.Shared;

namespace HUC.Web.App.Users.Courses
{
    public class UserCourseTestQuestionModel : BaseModel
    {
        public int UserCourseTestID { get; set; }
        public int TestQuestionID { get; set; }

        public bool IsFlagged { get; set; }
        public int? Attempts { get; set; }

        //Lazy
        private bool? _isCorrect;
        private IEnumerable<UserCourseTestQuestionAnswerModel> _userAnswers;
        private IEnumerable<TestQuestionAnswerModel> _testQuestionAnswers;
        private TestQuestionModel _testQuestion;
        private bool? _isAnswered;

        [DBIgnore]
        public bool IsCorrect
        {
            get
            {
                if (!_isCorrect.HasValue)
                {

                    

                    var correctAnswerIDs = this.TestQuestionAnswers.Where(x => x.IsCorrect).Select(x => x.ID);
                    var userAnswerIDs = this.UserAnswers.Select(x => x.TestAnswerID);

                    var correctString = String.Join(",", correctAnswerIDs.OrderBy(x => x));
                    var userAnswerString = String.Join(",", userAnswerIDs.OrderBy(x => x));

                    _isCorrect = correctString == userAnswerString;


                   


                }
                return _isCorrect.Value;
            }
            set { _isCorrect = value; }
        }

        [DBIgnore]
        public IEnumerable<UserCourseTestQuestionAnswerModel> UserAnswers
        {
            get
            {
                if (_userAnswers == null)
                {
                    _userAnswers = Database.GetAll<UserCourseTestQuestionAnswerModel>(
                        "WHERE UserCourseQuestionID = @ID", new {ID = this.ID});
                }
                return _userAnswers;
            }
            set { _userAnswers = value; }
        }

        [DBIgnore]
        public IEnumerable<TestQuestionAnswerModel> TestQuestionAnswers
        {
            get
            {
                if (_testQuestionAnswers == null)
                {
                    _testQuestionAnswers =
                        Database.GetAll<TestQuestionAnswerModel>("WHERE TestQuestionID = @TestQuestionID",
                            new {TestQuestionID = this.TestQuestionID});
                }
                return _testQuestionAnswers;
            }
            set { _testQuestionAnswers = value; }
        }

        [DBIgnore]
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

        [DBIgnore]
        public bool IsAnswered
        {
            get
            {
                if (!_isAnswered.HasValue)
                {
                    _isAnswered = this.UserAnswers.Any();
                }
                return _isAnswered.Value;
            }
            set { _isAnswered = value; }
        }
    }
}