using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AtlasDB;
using HUC.Web.App.Courses.Stages;
using HUC.Web.App.Resources;
using HUC.Web.App.Resources.Questions.Answers;
using HUC.Web.App.Shared;
using HUC.Web.App.Users.Courses;
namespace HUC.Web.App.Courses
{
    public class CourseModelFreetext : BaseModel
    {
        public bool IsPublished { get; set; }
        public bool IsDeleted { get; set; }

        [Required, StringLength(500), Display(Name = "Name")]
        public string Name { get; set; }

        [Display(Name = "Is Visible On Website?")]
        public bool IsVisibleWebsite { get; set; }

        [AllowHtml, Display(Name = "Course Description")]
        public string CourseDescription { get; set; }

        [AllowHtml, Display(Name = "About this course")]
        public string IntroCopy { get; set; }

        [AllowHtml, Display(Name = "Outro Text")]
        public string OutroCopy { get; set; }
        [Display(Name = "Background Image(1900 * 700)")]
        public string Background { get; set; }
        public bool BackGroundType { get; set; }
        [Display(Name = "Allow Re-Take?")]
        public bool? ReTake { get; set; }
        [Display(Name = "Pass/Fail Percentage")]
        public int? PassingPercentage { get; set; }
        [Display(Name = "Test Cool Down Hours")]
        public int? CoolDownHours { get; set; }
        [Display(Name = "Date Created")]
        public DateTime? CreatedDate { get; set; }
        [Display(Name = "Compliance Renewal")]
        public int? RetakeDuration { get; set; }
        //Lazy
        private IEnumerable<ResourceModel> _resources;
        private Dictionary<PublishabilityStatus, string> _publishStatus;
        private bool? _canBePublished;
        private CourseStageModel _firstStage;
        private IEnumerable<CourseStageModel> _stages;
        private int? _maxScore;

        [DBIgnore]
        public IEnumerable<ResourceModel> Resources
        {
            get
            {
                if (_resources == null)
                {
                    _resources =
                        Database.GetAll<ResourceModel>(
                            "WHERE CourseID = @ID AND IsDeleted = 0",
                            new { ID = this.ID });
                }
                return _resources;
            }
            set { _resources = value; }
        }

        [DBIgnore]
        public bool CanBePublished
        {
            get
            {
                if (_canBePublished == null)
                {
                    _canBePublished = this.PublishStatus.ContainsKey(PublishabilityStatus.OK);
                }
                return _canBePublished.Value;
            }
            set { _canBePublished = value; }
        }

        [DBIgnore]
        public Dictionary<PublishabilityStatus, string> PublishStatus
        {
            get
            {
                if (_publishStatus == null)
                {
                    var statuses = new Dictionary<PublishabilityStatus, string>();

                    //Check for resources
                    if (!this.Resources.Any())
                    {
                        //No resources
                        statuses.Add(PublishabilityStatus.NoResources, "");
                    }
                    else
                    {
                        //Resources found
                        foreach (var curResource in this.Resources)
                        {
                            //Check for chapters
                            if (!curResource.Chapters.Any())
                            {
                                //No chapters
                                if (statuses.ContainsKey(PublishabilityStatus.NoResourceChapters))
                                {
                                    statuses[PublishabilityStatus.NoResourceChapters] = statuses[PublishabilityStatus.NoResourceChapters] + ", " + curResource.Name;
                                }
                                else
                                {
                                    statuses.Add(PublishabilityStatus.NoResourceChapters, curResource.Name);
                                }
                            }
                            else
                            {
                                //Chapters found
                                foreach (var curChapter in curResource.Chapters)
                                {
                                    //Check for contents
                                    if (!curChapter.Contents.Any())
                                    {
                                        //No contents
                                        if (statuses.ContainsKey(PublishabilityStatus.NoResourceChapterContent))
                                        {
                                            statuses[PublishabilityStatus.NoResourceChapterContent] = statuses[PublishabilityStatus.NoResourceChapterContent] + ", " + curResource.Name + " (Chapter " + curChapter.Sort + ")";
                                        }
                                        else
                                        {
                                            statuses.Add(PublishabilityStatus.NoResourceChapterContent, curResource.Name + " (Chapter " + curChapter.Sort + ")");
                                        }
                                    }
                                    else
                                    {
                                        //Contents found
                                    }
                                }
                            }

                            //Check for questions
                            if (!curResource.Questions.Any())
                            {
                                //No questions
                                //if (statuses.ContainsKey(PublishabilityStatus.NoResourceQuestions))
                                //{
                                //    statuses[PublishabilityStatus.NoResourceQuestions] = statuses[PublishabilityStatus.NoResourceQuestions] + ", " + curResource.Name;
                                //}
                                //else
                                //{
                                //    statuses.Add(PublishabilityStatus.NoResourceQuestions, curResource.Name);
                                //}

                                //No questions is now allowed
                            }
                            else
                            {
                                //questions found
                                foreach (var curQuestion in curResource.Questions)
                                {
                                    //Check for answers
                                    if (curQuestion.Answers.Count() < 1)
                                    {
                                        //Not enough answers
                                        if (curQuestion.QuestionType == 7)
                                        {

                                        }
                                        else if (statuses.ContainsKey(PublishabilityStatus.NotEnoughResourceQuestionAnswers))
                                        {
                                            statuses[PublishabilityStatus.NotEnoughResourceQuestionAnswers] = statuses[PublishabilityStatus.NotEnoughResourceQuestionAnswers] + ", " + curResource.Name + " (Question " + curQuestion.Sort + ")";
                                        }
                                        else
                                        {
                                            statuses.Add(PublishabilityStatus.NotEnoughResourceQuestionAnswers, curResource.Name + " (Question " + curQuestion.Sort + ")");
                                        }
                                    }
                                    else
                                    {
                                        //Enough answers found

                                    }

                                    //Check that there is at least one correct answer provided
                                    if (curQuestion.Answers.Count(x => x.IsCorrect) < 0)
                                    {
                                        //No correct answers
                                        if (statuses.ContainsKey(PublishabilityStatus.NoResourceQuestionAnswerIsCorrect))
                                        {
                                            statuses[PublishabilityStatus.NoResourceQuestionAnswerIsCorrect] = statuses[PublishabilityStatus.NoResourceQuestionAnswerIsCorrect] + ", " + curResource.Name + " (Question " + curQuestion.Sort + ")";
                                        }
                                        else
                                        {
                                            statuses.Add(PublishabilityStatus.NoResourceQuestionAnswerIsCorrect, curResource.Name + " (Question " + curQuestion.Sort + ")");
                                        }
                                    }
                                }
                            }
                        }
                    }

                    if (!statuses.Any())
                    {
                        statuses.Add(PublishabilityStatus.OK, "");
                    }

                    _publishStatus = statuses;
                }
                return _publishStatus;
            }
            set { _publishStatus = value; }
        }

        [DBIgnore]
        public CourseStageModel FirstStage
        {
            get
            {
                if (_firstStage == null)
                {
                    _firstStage = this.Stages.OrderBy(x => x.Stage).FirstOrDefault();
                }
                return _firstStage;
            }
            set { _firstStage = value; }
        }

        [DBIgnore]
        public IEnumerable<CourseStageModel> Stages
        {
            get
            {
                if (_stages == null)
                {
                    _stages = Database.GetAll<CourseStageModel>("WHERE CourseID = @ID", new { ID = this.ID });
                }
                return _stages;
            }
            set { _stages = value; }
        }

        [DBIgnore]
        public int MaxScore
        {
            get
            {
                if (!_maxScore.HasValue)
                {
                    _maxScore = this.Resources.Sum(resource => resource.Questions.Count());
                    if (_maxScore == 0)
                    {
                        _maxScore = 1;
                    }
                }

                return _maxScore.Value;
            }
            set { _maxScore = value; }
        }

        public IEnumerable<UserCourseModel> UserCoursesInCompany(int companyID)
        {
            return Database.GetAll<UserCourseModel>(
                "WHERE CourseID = @CourseID AND UserID IN (SELECT UserID FROM CompanyUsers WHERE CompanyID = @CompanyID)", new
                {
                    CourseID = ID,
                    CompanyID = companyID
                }
            );
        }

        public static CourseModelFreetext GetIfInCompany(int courseID, int companyID)
        {
            var Database = new AtlasDatabase();

            var course = Database.GetAll<CourseModelFreetext>(
                    "WHERE ID = @CourseID " +
                    "AND (" +
                    "   ID IN (" +
                    "       SELECT CourseID FROM CompanyCourses WHERE CompanyID = @CompanyID" +
                    "   ) " +
                    "   OR ID IN (" +
                    "       SELECT CourseID FROM UserCourses WHERE UserID IN (" +
                    "           SELECT UserID FROM CompanyUsers WHERE CompanyID = @CompanyID" +
                    "       )" +
                    "   )" +
                    ")", new
                    {
                        CourseID = courseID,
                        CompanyID = companyID
                    });

            return course.FirstOrDefault();
        }

        public  List<TestQuestionAnswerEditModel> FreetextAnswerModule { get; set; }

    }
}