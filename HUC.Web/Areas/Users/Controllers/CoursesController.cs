using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Dapper;
using HUC.Web.App.Companies.Courses;
using HUC.Web.App.Companies.Users;
using HUC.Web.App.Courses;
using HUC.Web.App.Courses.Certificate;
using HUC.Web.App.Courses.Stages;
using HUC.Web.App.Heartbeats;
using HUC.Web.App.Resources;
using HUC.Web.App.Resources.Chapters;
using HUC.Web.App.Resources.Questions;
using HUC.Web.App.Resources.Questions.Answers;
using HUC.Web.App.Users;
using HUC.Web.App.Users.Courses;
using HUC.Web.Models;
using HUC.Web.Models.SinglePoint;
using HUC.Web.App.Companies;
using HUC.Web.App.Settings;
using System.Net.Mail;
using System.Configuration;
using HUC.Web.App.PageModels;

namespace HUC.Web.Areas.Users.Controllers
{
    public class CoursesController : UsersBaseController
    {
        public ActionResult Index()
        {

            //Grab the user
            var curUser = GetLoggedInUser();
            //employeesEntities.Database.Connection.ConnectionString = employeesEntities.Database.Connection.ConnectionString.Replace("DentonsEmployeesForRiskManager", "HUC");
            ViewBag.CourseList = Database.Query<int>("SELECT CourseID FROM AssignedCourses WHERE UserID = " + curUser.ID).ToList();
            return View(curUser);
        }

        public ActionResult Start(int id)
        {
            var curUser = GetLoggedInUser();
            var matchedCourse = curUser.CoursesReadyToStart.FirstOrDefault(x => x.ID == id);
            if (matchedCourse == null)
            {
                AddError("No Course Found!");
                return RedirectToAction("Index");
            }

            curUser.StartCourse(matchedCourse);


            AddNotification(NoteType.Info, "Welcome to, " + matchedCourse.Name);

            return RedirectToAction("Modules", new { id = matchedCourse.ID });
        }
        public ActionResult AboutCourse(int id)
        {
            var curUser = GetLoggedInUser();
            var curUserCourse = curUser.UserCourses.FirstOrDefault(x => x.CourseID == id && x.StartedOn.HasValue);

            //Check the user can access the course
            if (curUserCourse == null)
            {
                AddNotification(NoteType.Error, "You have not started that course!");
                return RedirectToAction("Index");
            }

            //Valid, log it as the last user course
            Database.ExecuteUpdate("Users", new[] { "LastUserCourseID" }, new { ID = curUser.ID, LastUserCourseID = curUserCourse.ID });

            //Ensure that the user is not in a test and exceeded the time permitted.
            //If so, ignore all commands and publish test results.
            if (curUserCourse.CourseStage.IsTest)
            {
                var curUserCourseTest = curUserCourse.CourseStage.Resource.LatestUserTestFor(curUserCourse.ID);
                var timeLimitMinutes = curUserCourse.CourseStage.Resource.TestTimeLimitMinutes;

                if (curUserCourseTest != null && timeLimitMinutes.HasValue && !curUserCourseTest.IsComplete && DateTime.Now > curUserCourseTest.StartOn.AddMinutes(timeLimitMinutes.Value))
                {
                    CompleteTest(curUserCourseTest.ID);
                    return RedirectToAction("View", new { id = id });
                }
            }

            if (curUserCourse.CourseStage.IsEnd && !curUserCourse.IsComplete)
            {
                //Next resource being null means this is the last resource. Now we mark the course as complete
                Database.ExecuteUpdate("UserCourses", new[] { "IsComplete", "CompleteOn" }, new { ID = curUserCourse.ID, IsComplete = true, CompleteOn = DateTime.Now });
            }
            return View(curUserCourse);
        }
        public ActionResult Assessment(int id, bool CourseEndModule = false)
        {
            var curUser = GetLoggedInUser();
            var curUserCourse = curUser.UserCourses.FirstOrDefault(x => x.CourseID == id && x.StartedOn.HasValue);

            //Check the user can access the course
            if (curUserCourse == null)
            {
                AddNotification(NoteType.Error, "You have not started that course!");
                return RedirectToAction("Index");
            }

            //Valid, log it as the last user course
            Database.ExecuteUpdate("Users", new[] { "LastUserCourseID" }, new { ID = curUser.ID, LastUserCourseID = curUserCourse.ID });

            //Ensure that the user is not in a test and exceeded the time permitted.
            //If so, ignore all commands and publish test results.
            if (curUserCourse.CourseStage.IsTest)
            {
                var curUserCourseTest = curUserCourse.CourseStage.Resource.LatestUserTestFor(curUserCourse.ID);
                var timeLimitMinutes = curUserCourse.CourseStage.Resource.TestTimeLimitMinutes;

                if (curUserCourseTest != null && timeLimitMinutes.HasValue && !curUserCourseTest.IsComplete && DateTime.Now > curUserCourseTest.StartOn.AddMinutes(timeLimitMinutes.Value))
                {
                    CompleteTest(curUserCourseTest.ID);
                    return RedirectToAction("View", new { id = id });
                }
            }

            if ((curUserCourse.CourseStage.IsEnd || CourseEndModule) && !curUserCourse.IsComplete)
            {
                //Next resource being null means this is the last resource. Now we mark the course as complete
                Database.ExecuteUpdate("UserCourses", new[] { "IsComplete", "CompleteOn" }, new { ID = curUserCourse.ID, IsComplete = true, CompleteOn = DateTime.Now });
            }
            curUserCourse.ColleagueId = curUserCourse.ColleagueId == null ? 0 : curUserCourse.ColleagueId;
            curUserCourse.ClientId = curUserCourse.ClientId == null ? 0 : curUserCourse.ClientId;
            curUserCourse.WorkId = curUserCourse.WorkId == null ? 0 : curUserCourse.WorkId;
            return View(curUserCourse);
        }
        [HttpPost]
        public ActionResult Assessment(int id, int ColleagueId = 0, int ClientId = 0, int WorkId = 0)
        {
            var curUser = GetLoggedInUser();
            var curUserCourse = curUser.UserCourses.FirstOrDefault(x => x.CourseID == id && x.StartedOn.HasValue);

            //Check the user can access the course
            if (curUserCourse == null)
            {
                AddNotification(NoteType.Error, "You have not started that course!");
                return RedirectToAction("Index");
            }

            //Valid, log it as the last user course
            Database.ExecuteUpdate("Users", new[] { "LastUserCourseID" }, new { ID = curUser.ID, LastUserCourseID = curUserCourse.ID });

            //Ensure that the user is not in a test and exceeded the time permitted.
            //If so, ignore all commands and publish test results.
            if (curUserCourse.CourseStage.IsTest)
            {
                var curUserCourseTest = curUserCourse.CourseStage.Resource.LatestUserTestFor(curUserCourse.ID);
                var timeLimitMinutes = curUserCourse.CourseStage.Resource.TestTimeLimitMinutes;

                if (curUserCourseTest != null && timeLimitMinutes.HasValue && !curUserCourseTest.IsComplete && DateTime.Now > curUserCourseTest.StartOn.AddMinutes(timeLimitMinutes.Value))
                {
                    CompleteTest(curUserCourseTest.ID);
                    return RedirectToAction("View", new { id = id });
                }
            }
            if (ColleagueId > 0)
            {
                curUserCourse.ColleagueId = ColleagueId;
                Database.ExecuteUpdate(curUserCourse);
            }
            else if (ClientId > 0)
            {
                curUserCourse.ClientId = ClientId;
                Database.ExecuteUpdate(curUserCourse);
            }
            else if (WorkId > 0)
            {
                curUserCourse.WorkId = WorkId;
                Database.ExecuteUpdate(curUserCourse);
            }
            curUserCourse = curUser.UserCourses.FirstOrDefault(x => x.CourseID == id && x.StartedOn.HasValue);
            if (curUserCourse.ColleagueId > 0 && curUserCourse.ClientId > 0 && curUserCourse.WorkId > 0)
            {
                //Next resource being null means this is the last resource. Now we mark the course as complete
                Database.ExecuteUpdate("UserCourses", new[] { "IsComplete", "CompleteOn" }, new { ID = curUserCourse.ID, IsComplete = true, CompleteOn = DateTime.Now });
                return Json(new { IsComplete = true, Feedback = true, Message = "Answer submitted successfuly." }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { IsComplete = false, Feedback = true, Message = "Answer submitted successfuly." }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult Modules(int id)
        {
            var curUser = GetLoggedInUser();
            var curUserCourse = curUser.UserCourses.FirstOrDefault(x => x.CourseID == id && x.StartedOn.HasValue);

            //Check the user can access the course
            if (curUserCourse == null)
            {
                AddNotification(NoteType.Error, "You have not started that course!");
                return RedirectToAction("Index");
            }

            //Valid, log it as the last user course
            Database.ExecuteUpdate("Users", new[] { "LastUserCourseID" }, new { ID = curUser.ID, LastUserCourseID = curUserCourse.ID });

            //Ensure that the user is not in a test and exceeded the time permitted.
            //If so, ignore all commands and publish test results.
            if (curUserCourse.CourseStage.IsTest)
            {
               
                var curUserCourseTest = curUserCourse.CourseStage.Resource.LatestUserTestFor(curUserCourse.ID);
                var timeLimitMinutes = curUserCourse.CourseStage.Resource.TestTimeLimitMinutes;
                

                if (curUserCourseTest != null && timeLimitMinutes.HasValue && !curUserCourseTest.IsComplete && DateTime.Now > curUserCourseTest.StartOn.AddMinutes(timeLimitMinutes.Value))
                {
                    CompleteTest(curUserCourseTest.ID);
                    return RedirectToAction("View", new { id = id });
                }
                //if user attempted question and Current course question are Equels it mean user course is complete

                //if (curUserCourseTest.TestQuestions.Count() == curUserCourseTest.UserQuestions.Count() && !curUserCourseTest.IsComplete)
                if (curUserCourse.TotalAttemtedQuestion == curUserCourse.Course.MaxScore && !curUserCourseTest.IsComplete)
                {
                    Database.ExecuteUpdate("UserCourses", new[] { "IsComplete", "CompleteOn" }, new { ID = curUserCourse.ID, IsComplete = true, CompleteOn = DateTime.Now });
                    CompleteTest(curUserCourseTest.ID);
                }
            }

            if (curUserCourse.CourseStage.IsEnd && !curUserCourse.IsComplete)
            {
                //Next resource being null means this is the last resource. Now we mark the course as complete
                Database.ExecuteUpdate("UserCourses", new[] { "IsComplete", "CompleteOn" }, new { ID = curUserCourse.ID, IsComplete = true, CompleteOn = DateTime.Now });
            }
            if (curUserCourse.Course.Resources.OrderByDescending(x => x.Sort).FirstOrDefault().Questions != null)
            {


                //if (curUserCourseTest.UserQuestions != null)
                //    if (curUserCourse.Course.Resources.OrderByDescending(x => x.Sort).FirstOrDefault().Questions.Count() == curUserCourseTest.UserQuestions.Where(x => curUserCourse.Course.Resources.OrderByDescending(x => x.Sort).FirstOrDefault().Questions.Select(y => y.ID).Contains(x.TestQuestionID)).Count())
                //    {
                //        curUserCourse.Course.Resources.OrderByDescending(x => x.Sort).FirstOrDefault().ModuleCompleted = true;
                //    }
            }
            else
            {
                curUserCourse.Course.Resources.OrderByDescending(x => x.Sort).FirstOrDefault().ModuleCompleted = true;
            }
            return View(curUserCourse);
        }
        public ActionResult ModuleDetail(int id)
        {
            int totalResource = 3;
            var resourceDetail = Database.GetSingle<ResourceModel>(id);
            int lastStageIdByModule = resourceDetail.Course.Stages.Where(x => x.ResourceID == id).OrderByDescending(x => x.Stage).Select(x => x.ID).FirstOrDefault();
            var currentResource = resourceDetail;
            var curUser = GetLoggedInUser();
            TempData["CurrentUserId"] = curUser.ID;
            var curUserCourse = curUser.UserCourses.FirstOrDefault(x => x.CourseID == resourceDetail.Course.ID && x.StartedOn.HasValue);
            
            while (currentResource.PrevResource != null)
            {
                totalResource++;
                currentResource = currentResource.PrevResource;
            }
            currentResource = resourceDetail;
            while (currentResource.NextResource != null)
            {
                totalResource++;
                currentResource = currentResource.NextResource;
            }
            resourceDetail.TotalResource = totalResource;
            if (lastStageIdByModule >= curUserCourse.CourseStageID)
            {
                curUserCourse.CourseStageID = lastStageIdByModule;
            }
            Database.ExecuteUpdate(curUserCourse);
            //var curUserCourseTest = curUserCourse.CourseStage.Resource.LatestUserTestFor(curUserCourse.ID);
            //int answerCounter = 0;
            //foreach (var question in resourceDetail.Questions)
            //{
            //    var curTestQuestion = curUserCourseTest.UserQuestions.Single(x => x.TestQuestionID == question.ID);
            //    //int result = Database.Query<int>("SELECT COUNT(ID) AS [Count] FROM UserCourseTestQuestionAnswers WHERE [UserCourseQuestionID] = (SELECT ID FROM UserCourseTestQuestions WHERE TestQuestionID = " + question.ID + " )").FirstOrDefault();
            //    //answerCounter += result;
            //}
            //if (answerCounter == resourceDetail.Questions.Count())
            //{
            //    resourceDetail.ModuleCompleted = true;
            //}
            //result.
            var curUserCourseTest = new UserCourseTestModel();
            if (curUserCourse.CourseStage != null)
            {

                //curUserCourseTest = curUserCourse.CourseStage.Resource.LatestUserTestFor(curUserCourse.ID);
                curUserCourseTest = curUserCourse.Course.Resources.Where(x => x.ID == id).FirstOrDefault().LatestUserTestFor(curUserCourse.ID);   //.CourseStage.Resource.LatestUserTestFor(curUserCourse.ID);
                if (curUserCourseTest == null)
                {
                    UserCourseTestModel _userCourseTestModel = new UserCourseTestModel();
                    _userCourseTestModel.UserCourseID = curUserCourse.ID;
                    _userCourseTestModel.ResourceID = id;
                    _userCourseTestModel.StartOn = DateTime.Now;
                    _userCourseTestModel.IsComplete = false;
                    Database.ExecuteInsert(_userCourseTestModel);
                    curUserCourseTest = curUserCourse.CourseStage.Resource.LatestUserTestFor(curUserCourse.ID);
                }
            }
            //var curUserQuestions = curUserCourseTest.UserQuestions.Where(x => x.TestQuestionID == 371).FirstOrDefault();
            //var selected = curUserQuestions.UserAnswers.Where(x => x.TestAnswerID == 1154).ToList().Count() > 0 ? true : false; 
            List<TestQuestionModel> tempTestQuestionModels = new List<TestQuestionModel>();
            foreach (var question in resourceDetail.Questions)
            {
                var curUserQuestions = curUserCourseTest.UserQuestions.Where(x => x.TestQuestionID == question.ID).FirstOrDefault();
                TestQuestionModel tempTestQuestion = new TestQuestionModel();
                tempTestQuestion = question;
                if (curUserQuestions != null)
                {
                    tempTestQuestion.Attempts = curUserQuestions.Attempts == null ? tempTestQuestion.Attempts : curUserQuestions.Attempts;
                    tempTestQuestion.AnswerId = curUserQuestions.UserAnswers.Select(x => x.TestAnswerID).ToList();
                     tempTestQuestion.AnswerOrder = question.QuestionType != 4 ? question.Answers.Where(x => x.ID == tempTestQuestion.AnswerId.FirstOrDefault()).Select(x => x.Sort).FirstOrDefault() : 0;

                  


                    if (question.QuestionType == 4)
                    {
                        if (tempTestQuestion.AnswerId.Count() == 0)
                        {
                            tempTestQuestion.AnswerId.Add(0);
                        }
                    }
                }
                else
                {
                   // tempTestQuestion.Attempts = 2;
                    tempTestQuestion.AnswerId = new List<int>();
                }
                tempTestQuestionModels.Add(tempTestQuestion);
            }
            resourceDetail.Questions = tempTestQuestionModels;
            if (resourceDetail.Questions != null)
            {
                if (curUserCourseTest.UserQuestions != null)
                    if (resourceDetail.Questions.Count() == curUserCourseTest.UserQuestions.Where(x => resourceDetail.Questions.Select(y => y.ID).Contains(x.TestQuestionID)).Count())
                    {
                        resourceDetail.ModuleCompleted = true;
                    }
            }
            else
            {
                resourceDetail.ModuleCompleted = true;
            }
            //for (int i = 0; i < resourceDetail.Questions.Count(); i++)
            //{
            //    var curUserQuestions = curUserCourseTest.UserQuestions.Where(x => x.TestQuestionID == 371).FirstOrDefault();
            //}


            //admin preview 


            var user = Database.GetSingle<UserModel>(curUser.ID);
            int freetextanswer = 0;

            var resource = Database.GetSingle<ResourceModel>(id);
            freetextanswer = 0;
            ViewBag.freetextCount = 0;

            var FreeText = resource.Questions.Where(w => w.QuestionType == 7);

            foreach (var q in FreeText)
            {


                var freetextanswers = q.Answers.Where(w => w.TestQuestionID == q.ID && w.IsCorrect == true && user.ID == w.CourseUserId && w.IsResult == 0).ToList();
              //  var freetextcount = q.Answers.Where(w => w.TestQuestionID == q.ID && user.ID == w.CourseUserId).ToList();

                //if (!curUserCourse.IsPass)
                //{
                //    ViewBag.freetextCount = freetextcount.Count;
                //}


                if (freetextanswers.Count() > 0)
                {

                    freetextanswer = freetextanswers.Count();
                    ViewBag.adminAns = 1;
                }
            }



           




            return View(resourceDetail);
        }
        public ActionResult View(int id)
        {
            var curUser = GetLoggedInUser();
            var curUserCourse = curUser.UserCourses.FirstOrDefault(x => x.CourseID == id && x.StartedOn.HasValue);

            //Check the user can access the course
            if (curUserCourse == null)
            {
                AddNotification(NoteType.Error, "You have not started that course!");
                return RedirectToAction("Index");
            }

            //Valid, log it as the last user course
            Database.ExecuteUpdate("Users", new[] { "LastUserCourseID" }, new { ID = curUser.ID, LastUserCourseID = curUserCourse.ID });

            //Ensure that the user is not in a test and exceeded the time permitted.
            //If so, ignore all commands and publish test results.
            if (curUserCourse.CourseStage.IsTest)
            {
                var curUserCourseTest = curUserCourse.CourseStage.Resource.LatestUserTestFor(curUserCourse.ID);
                var timeLimitMinutes = curUserCourse.CourseStage.Resource.TestTimeLimitMinutes;

                if (curUserCourseTest != null && timeLimitMinutes.HasValue && !curUserCourseTest.IsComplete && DateTime.Now > curUserCourseTest.StartOn.AddMinutes(timeLimitMinutes.Value))
                {
                    CompleteTest(curUserCourseTest.ID);
                    return RedirectToAction("View", new { id = id });
                }
            }

            if (curUserCourse.CourseStage.IsEnd && !curUserCourse.IsComplete)
            {
                //Next resource being null means this is the last resource. Now we mark the course as complete
                Database.ExecuteUpdate("UserCourses", new[] { "IsComplete", "CompleteOn" }, new { ID = curUserCourse.ID, IsComplete = true, CompleteOn = DateTime.Now });
            }
            ViewBag.FullName = curUser.FirstName + " " + curUser.LastName;
            ViewBag.CompanyName = curUser.Company.Name;
            return View(curUserCourse);
        }

        public ActionResult Stage(int id, string changeStage, int? destStage = null)
        {
            var curUser = GetLoggedInUser();
            var curUserCourse = curUser.UserCourses.FirstOrDefault(x => x.CourseID == id && x.StartedOn.HasValue);

            //Check the course is null
            if (curUserCourse == null)
            {
                AddNotification(NoteType.Error, "You have not started that course!");
                return RedirectToAction("Index");
            }

            UserCourseTestModel curUserCourseTest = null;
            if (curUserCourse.CourseStage.IsTest)
            {
                curUserCourseTest = curUserCourse.CourseStage.Resource.LatestUserTestFor(curUserCourse.ID);
            }

            //Log a heartbeat of their current stage to fill in the gaps
            //TO BE REMOVED
            //Beat(curUserCourse.ID, curUserCourseTest == null ? (int?)null : curUserCourseTest.ID, curUserCourse.CourseStage.ChapterID);
            //--

            //Ensure that the user is not in a test and exceeded the time permitted.
            //If so, ignore all commands and publish test results.
            if (curUserCourse.CourseStage.IsTest)
            {
                var timeLimitMinutes = curUserCourse.CourseStage.Resource.TestTimeLimitMinutes;

                if (curUserCourseTest != null && timeLimitMinutes.HasValue && !curUserCourseTest.IsComplete && DateTime.Now > curUserCourseTest.StartOn.AddMinutes(timeLimitMinutes.Value))
                {
                    CompleteTest(curUserCourseTest.ID);

                    return RedirectToAction("View", new { id = id });
                }
            }

            if (changeStage.ToLower() == "prev")
            {
                //Ensure that we are not in the middle of a test.
                if (!curUserCourse.CourseStage.IsTest || ((curUserCourse.CourseStage.IsTestEnd && curUserCourseTest != null && curUserCourseTest.IsComplete) || curUserCourse.CourseStage.IsTestStart))
                {
                    var prevStage = curUserCourse.CourseStage.PrevStage;
                    if (prevStage == null)
                    {
                        AddNotification(NoteType.Error, "There is no Next Stage available.");
                    }
                    else
                    {
                        if (curUserCourse.CourseStage.IsTestEnd)
                        {
                            //We want to jump to the test start
                            curUserCourse.CourseStageID =
                                Database.Query<int>(
                                    "SELECT ID FROM CourseStages WHERE CourseID = @CourseID AND ResourceID = @ResourceID AND IsTestStart = 1",
                                    new { CourseID = curUserCourse.CourseID, ResourceID = curUserCourse.CourseStage.ResourceID })
                                    .First();
                            Database.ExecuteUpdate(curUserCourse);
                        }
                        else
                        {
                            curUserCourse.CourseStageID = prevStage.ID;
                            Database.ExecuteUpdate(curUserCourse);
                        }
                    }
                }
                else
                {
                    AddNotification(NoteType.Error, "Action Unavailable");
                    return RedirectToAction("View", new { id = id });
                }
            }
            else if (changeStage.ToLower() == "next")
            {
                //Ensure that we are not in the middle of a test.
                if (curUserCourse.CourseStage.IsTest)
                {
                    if (curUserCourse.CourseStage.IsTestStart)
                    {
                        //Ensure cooldown compliance!
                        var cooldownHours = curUserCourse.CourseStage.Resource.TestCooldownHours;
                        var timeLimitMinutes = curUserCourse.CourseStage.Resource.TestTimeLimitMinutes;
                        if (curUserCourseTest != null)
                        {
                            if (cooldownHours.HasValue)
                            {
                                var offsetMinutes = timeLimitMinutes.HasValue ? timeLimitMinutes.Value : 0;
                                var cooldownBound = curUserCourseTest.StartOn.AddHours((double)cooldownHours.Value).AddMinutes(offsetMinutes);

                                if (DateTime.Now < cooldownBound)
                                {
                                    //Too Soon!
                                    AddNotification(NoteType.Error, string.Format("You must wait until {0} at {1} to retake this test", cooldownBound.ToLongDateString(), cooldownBound.ToShortTimeString()));
                                    return RedirectToAction("View", new { id = id });
                                }
                            }
                            else
                            {
                                //There is a null cooldown which signifies you may not retake the test.
                                AddNotification(NoteType.Error, "You may not retake this test.");
                                return RedirectToAction("View", new { id = id });
                            }
                        }

                        //The test may be started
                    }
                    else if (curUserCourse.CourseStage.IsTestEnd)
                    {
                        if (curUserCourseTest.IsComplete)
                        {
                            //Allow
                        }
                        else
                        {
                            //Allow, however we will not be changing stage here. We will only be setting the test as complete and returning to the page.
                            CompleteTest(curUserCourseTest.ID);
                            return RedirectToAction("View", new { id = id });
                        }
                    }
                    else
                    {
                        //Not allowed
                        AddNotification(NoteType.Error, "Action Unavailable");
                        return RedirectToAction("View", new { id = id });
                    }
                }

                var nextStage = curUserCourse.CourseStage.NextStage;
                if (nextStage == null)
                {
                    AddNotification(NoteType.Error, "There is no Next Stage available.");
                }
                else
                {
                    if (curUserCourse.CourseStage.IsTestStart)
                    {
                        var newTest = new UserCourseTestModel
                        {
                            UserCourseID = curUserCourse.ID,
                            ResourceID = curUserCourse.CourseStage.ResourceID.Value,
                            StartOn = DateTime.Now,
                            IsComplete = false
                        };
                        var newTestID = Database.ExecuteInsert(newTest, true);

                        //Now iterate the test questions and create basic data versions of these against the test
                        foreach (var curQuestion in curUserCourse.CourseStage.Resource.Questions)
                        {
                            var newQuestion = new UserCourseTestQuestionModel
                            {
                                UserCourseTestID = newTestID,
                                TestQuestionID = curQuestion.ID,
                                IsFlagged = false
                            };
                            Database.ExecuteInsert(newQuestion);
                        }
                    }

                    curUserCourse.CourseStageID = nextStage.ID;
                    Database.ExecuteUpdate(curUserCourse);
                }
            }
            else if (changeStage.ToLower() == "skiptest")
            {
                //Ensure that we are only at a test start!
                if (curUserCourse.CourseStage.IsTestStart)
                {
                    var curUserTest = curUserCourse.CourseStage.Resource.LatestUserTestFor(curUserCourse.ID);

                    //Ensure that the test has actually been completed before allowing the skip action
                    if (curUserTest != null && curUserTest.IsComplete)
                    {
                        //We want to jump to the test end + 1
                        var testEndStage = Database.GetAll<CourseStageModel>("WHERE CourseID = @CourseID AND ResourceID = @ResourceID AND IsTestEnd = 1", new { CourseID = curUserCourse.Course.ID, resourceID = curUserCourse.CourseStage.ResourceID }).SingleOrDefault();
                        if (testEndStage == null)
                        {
                            AddNotification(NoteType.Error, "Action Unavailable");
                            return RedirectToAction("View", new { id = id });
                        }
                        else
                        {
                            curUserCourse.CourseStageID = testEndStage.NextStage.ID;
                            Database.ExecuteUpdate(curUserCourse);
                        }

                    }
                    else
                    {
                        //Diallow the change!
                        AddNotification(NoteType.Error, "You must complete the test before you may skip it.");
                        return RedirectToAction("View", new { id = id });
                    }
                }
                else
                {
                    AddNotification(NoteType.Error, "Action Unavailable");
                    return RedirectToAction("View", new { id = id });
                }
            }
            else if (changeStage.ToLower() == "skipto")
            {
                var destStageO = Database.GetSingle<CourseStageModel>(destStage);
                if (destStageO == null || destStageO.CourseID != curUserCourse.CourseID || !destStageO.CanBeSkippedTo(curUserCourse.ID))
                {
                    AddNotification(NoteType.Error, "Invalid Destination Stage");
                    return RedirectToAction("View", new { id = id });
                }

                curUserCourse.CourseStageID = destStageO.ID;
                Database.ExecuteUpdate(curUserCourse);
            }
            else if (changeStage.ToLower() == "restartresource")
            {
                if (!curUserCourse.CourseStage.IsTestEnd)
                {
                    AddNotification(NoteType.Error, "You cannot restart the resource from here.");
                    return RedirectToAction("View", new { id = id });
                }

                var startItem =
                    Database.Query<CourseStageModel>(
                        "SELECT TOP 1 * FROM CourseStages WHERE CourseID = @CourseID AND ResourceID = @ResourceID ORDER BY Stage ASC",
                        new { CourseID = curUserCourse.CourseID, ResourceID = curUserCourse.CourseStage.ResourceID }).Single();

                curUserCourse.CourseStageID = startItem.ID;
                Database.ExecuteUpdate(curUserCourse);
            }
            else
            {
                AddNotification(NoteType.Error, "Action Unknown");
                return RedirectToAction("View", new { id = id });
            }

            return RedirectToAction("View", new { id = id });
        }

        [HttpPost]
        public JsonResult SubmitAnswer(int id, int course, IEnumerable<int> answer, int question = 0, bool flagged = false, string FreeTextAnswer = "")
        {
            bool muduleCompleted = false;
            int ansid = 0;
            var curUser = GetLoggedInUser();
            var curUserCourse = curUser.UserCourses.FirstOrDefault(x => x.CourseID == course && x.StartedOn.HasValue);

            if (curUserCourse == null)
            {
                //AddNotification(NoteType.Error, "You have not started that course!");
                return Json(new { Feedback = false, Message = "You have not started that course!" }, JsonRequestBehavior.AllowGet);
            }

            //Ensure that we are on a question
            //if (curUserCourse.CourseStage.TestQuestion == null)
            //{
            //    //Not a question, return to view.
            //    return RedirectToAction("ModuleDetail", new { id = id });
            //}
           
            //Ensure that a test has been started
            var curUserCourseTest = curUserCourse.CourseStage.Resource.LatestUserTestFor(curUserCourse.ID);
            if (curUserCourseTest == null)
            {
                var CurrentQuestion = Database.GetSingle<TestQuestionModel>(question);
                UserCourseTestModel _userCourseTestModel = new UserCourseTestModel();
                _userCourseTestModel.UserCourseID = curUserCourse.ID;
                _userCourseTestModel.ResourceID = id;
                _userCourseTestModel.StartOn = DateTime.Now;
                _userCourseTestModel.IsComplete = false;
                Database.ExecuteInsert(_userCourseTestModel);
                curUserCourseTest = curUserCourse.CourseStage.Resource.LatestUserTestFor(curUserCourse.ID);
                UserCourseTestQuestionModel _userCourseTestQuestionModel = new UserCourseTestQuestionModel();
                _userCourseTestQuestionModel.UserCourseTestID = curUserCourseTest.ID;
                _userCourseTestQuestionModel.TestQuestionID = question;
                _userCourseTestQuestionModel.IsFlagged = false;
                _userCourseTestQuestionModel.Attempts = CurrentQuestion.Attempts - 1;
                if (_userCourseTestQuestionModel.Attempts >= 0)
                    Database.ExecuteInsert(_userCourseTestQuestionModel);
                curUserCourseTest = curUserCourse.CourseStage.Resource.LatestUserTestFor(curUserCourse.ID);
            }
            else
            {
                var curUserQuestions = curUserCourseTest.UserQuestions.Where(x => x.TestQuestionID == question).FirstOrDefault();
                if (curUserQuestions == null)
                {
                    var CurrentQuestion = Database.GetSingle<TestQuestionModel>(question);
                    UserCourseTestQuestionModel _userCourseTestQuestionModel = new UserCourseTestQuestionModel();
                    _userCourseTestQuestionModel.UserCourseTestID = curUserCourseTest.ID;
                    _userCourseTestQuestionModel.TestQuestionID = question;
                    _userCourseTestQuestionModel.IsFlagged = false;
                    _userCourseTestQuestionModel.Attempts = CurrentQuestion.Attempts - 1;
                    if (_userCourseTestQuestionModel.Attempts >= 0)
                        Database.ExecuteInsert(_userCourseTestQuestionModel);
                    curUserCourseTest = curUserCourse.CourseStage.Resource.LatestUserTestFor(curUserCourse.ID);
                }
                else
                {
                    curUserQuestions.Attempts -= 1;
                    if(curUserQuestions.Attempts >= 0)
                       Database.ExecuteUpdate(curUserQuestions);
                    curUserCourseTest = curUserCourse.CourseStage.Resource.LatestUserTestFor(curUserCourse.ID);
                }
            }
            //Ensure that the user is not in a test and exceeded the time permitted.
            //If so, ignore all commands and publish test results.
            var timeLimitMinutes = curUserCourse.CourseStage.Resource.TestTimeLimitMinutes;

            if (timeLimitMinutes.HasValue && !curUserCourseTest.IsComplete && DateTime.Now > curUserCourseTest.StartOn.AddMinutes(timeLimitMinutes.Value))
            {
                CompleteTest(curUserCourseTest.ID);
                //return RedirectToAction("ModuleDetail", new { id = id });
                //return Json(new { Feedback = false, Message = "You have not started that course!" }, JsonRequestBehavior.AllowGet);
            }

            var curTestQuestion =  curUserCourseTest.UserQuestions.Single(x => x.TestQuestionID == question);

           
            //Valid post from question
            if (answer != null && answer.Any())
            {
                //Answers provided, we will update the database entries.
                if (FreeTextAnswer != null && FreeTextAnswer != "")
                {
                  ansid =  AddEditAnswers(FreeTextAnswer, question, "0" , curUser.ID);
                }
                //Ensure that any answer ID's provided exist in the valid answer ID's for this question
                //if (answer.Any(answerID => !curUserCourse.CourseStage.TestQuestion.Answers.Any(x => x.ID == answerID)))
                //{
                //    AddError("Invalid answer provided.");

                //    return RedirectToAction("ModuleDetail", new { id = id });
                //}

                Database.Execute("DELETE FROM UserCourseTestQuestionAnswers WHERE UserCourseQuestionID = @UserCourseQuestionID", new { UserCourseQuestionID = curTestQuestion.ID });
               
                foreach (var answerID in answer)
                {
                    if(answerID == 0)
                    {
                        if(ansid > 0)
                        {
                            var curAnswer = new UserCourseTestQuestionAnswerModel
                            {
                                UserCourseQuestionID = curTestQuestion.ID,
                                TestAnswerID = ansid
                            };
                        }
                    }
                    else
                    {
                    var curAnswer = new UserCourseTestQuestionAnswerModel
                    {
                        UserCourseQuestionID = curTestQuestion.ID,
                        TestAnswerID = answerID
                    };
                    Database.ExecuteInsert(curAnswer);
                    }
                }
            }
            

            curTestQuestion.IsFlagged = flagged;
            Database.ExecuteUpdate(curTestQuestion);
            var resourceDetail = Database.GetSingle<ResourceModel>(id);
            
            if (resourceDetail.Questions.Count() == curUserCourseTest.UserQuestions.Where(x => resourceDetail.Questions.Select(y => y.ID).Contains(x.TestQuestionID)).Count())
            {
                
                muduleCompleted = true;
                //If this module is last module
                //Update course is completed
                if (resourceDetail.NextResource == null)
                {
                    Assessment(course, true);
                }
            }
          
            int sssd = 0;
            foreach (var curResource in curUserCourse.Course.Resources)
            {
                var curTest = curResource.LatestUserTestFor(curUserCourse.ID);


                if (curTest != null && curTest.UserQuestions.Count() > 0)
                {
                    sssd += curTest.UserQuestions.Count();

                }

            }
            //if user attempted all question and Current course question are Equels it mean user course is complete
            // if (curUserCourseTest.TestQuestions.Count() == curUserCourseTest.UserQuestions.Count() && !curUserCourseTest.IsComplete)

            //OMER: Commedted below because not needed, we already completing course if last question submitted for last module/ Assessment(course, true);
            if (sssd == curUserCourse.Course.MaxScore && !curUserCourseTest.IsComplete)
            {
            //    Database.ExecuteUpdate("UserCourses", new[] { "IsComplete", "CompleteOn" }, new { ID = curUserCourse.ID, IsComplete =false, CompleteOn = DateTime.Now });
                CompleteTest(curUserCourseTest.ID);
            }

            //curUserCourse.CourseStageID = curUserCourse.CourseStage.NextStage.ID;
            //Database.ExecuteUpdate(curUserCourse);

            //AddInfo("Provided Answers: " + String.Join(", ", Answer));

            //return RedirectToAction("ModuleDetail", new { id = id });
            return Json(new { IsComplete = muduleCompleted, Feedback = true, Message = "Answer submitted successfuly." }, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult View(int id, IEnumerable<int> answer, bool flagged = false)
        {
            var curUser = GetLoggedInUser();
            var curUserCourse = curUser.UserCourses.FirstOrDefault(x => x.CourseID == id && x.StartedOn.HasValue);

            if (curUserCourse == null)
            {
                AddNotification(NoteType.Error, "You have not started that course!");
                return RedirectToAction("Index");
            }

            //Ensure that we are on a question
            if (curUserCourse.CourseStage.TestQuestion == null)
            {
                //Not a question, return to view.
                return RedirectToAction("View", new { id = id });
            }

            //Ensure that a test has been started
            var curUserCourseTest = curUserCourse.CourseStage.Resource.LatestUserTestFor(curUserCourse.ID);
            if (curUserCourseTest == null)
            {
                return RedirectToAction("View", new { id = id });
            }

            //Ensure that the user is not in a test and exceeded the time permitted.
            //If so, ignore all commands and publish test results.
            var timeLimitMinutes = curUserCourse.CourseStage.Resource.TestTimeLimitMinutes;

            if (timeLimitMinutes.HasValue && !curUserCourseTest.IsComplete && DateTime.Now > curUserCourseTest.StartOn.AddMinutes(timeLimitMinutes.Value))
            {
                CompleteTest(curUserCourseTest.ID);
                return RedirectToAction("View", new { id = id });
            }

            var curTestQuestion = curUserCourseTest.UserQuestions.Single(x => x.TestQuestionID == curUserCourse.CourseStage.TestQuestionID);

            //Valid post from question
            if (answer != null && answer.Any())
            {
                //Answers provided, we will update the database entries.

                //Ensure that any answer ID's provided exist in the valid answer ID's for this question
                if (answer.Any(answerID => !curUserCourse.CourseStage.TestQuestion.Answers.Any(x => x.ID == answerID)))
                {
                    AddError("Invalid answer provided.");

                    return RedirectToAction("View", new { id = id });
                }

                Database.Execute("DELETE FROM UserCourseTestQuestionAnswers WHERE UserCourseQuestionID = @UserCourseQuestionID", new { UserCourseQuestionID = curTestQuestion.ID });
                foreach (var answerID in answer)
                {
                    var curAnswer = new UserCourseTestQuestionAnswerModel
                    {
                        UserCourseQuestionID = curTestQuestion.ID,
                        TestAnswerID = answerID
                    };
                    Database.ExecuteInsert(curAnswer);
                }
            }

            curTestQuestion.IsFlagged = flagged;
            Database.ExecuteUpdate(curTestQuestion);


            curUserCourse.CourseStageID = curUserCourse.CourseStage.NextStage.ID;
            Database.ExecuteUpdate(curUserCourse);

            //AddInfo("Provided Answers: " + String.Join(", ", Answer));

            return RedirectToAction("View", new { id = id });
        }

        private void CompleteTest(int id)
        {
            var now = DateTime.Now;

            var userCourseTestEdit = Database.GetSingle<UserCourseTestModel>(id);
          
            
                userCourseTestEdit.IsComplete = true;
            
           
            userCourseTestEdit.CompleteOn = now;
            Database.ExecuteUpdate(userCourseTestEdit);
            Database.Execute("Insert into UserComletedCourseDates (UserCourseID, CompletedDate) Values(@UserCourseID, @CompletedDate)",
                new { UserCourseID = userCourseTestEdit.UserCourseID, CompletedDate = now });
            if(/*userCourseTestEdit.CorrectAnswerCount != userCourseTestEdit.UserQuestions.Count() &&*/ userCourseTestEdit.UserCourse.Course.ReTake != null && userCourseTestEdit.UserCourse.Course.ReTake == true && userCourseTestEdit.UserCourse.Course.CoolDownHours != null)
            {
                Database.Execute("update UserCourses set CoolDownHoursTime = @CoolDownHoursTime where ID = @ID", new { CoolDownHoursTime = DateTime.Now.AddHours(Convert.ToDouble(userCourseTestEdit.UserCourse.Course.CoolDownHours)), ID = userCourseTestEdit.UserCourseID });
            }
        }

        public JsonResult Heartbeat(int id, bool test = false)
        {
            var response = new HeartbeatResponseModel();
            //The hearbeat acts as 2 things for the system. First is to log time spent on a particular course, second is to ensure that a user is not trying to cheat and that they are still presently on their computer.
            //The heartbeat's response will be responsible for closing a users's 'session' in a course.

            //var curCompanyCourse = Database.GetSingle<CompanyCourseModel>(id);
            var curUser = GetLoggedInUser();
            var curUserCourse = curUser.UserCourses.FirstOrDefault(x => x.CourseID == id && x.StartedOn.HasValue);

            if (curUserCourse == null)
            {
                response.ForceClose = true;
                return Json(response, JsonRequestBehavior.AllowGet);
            }

            if (curUserCourse.CourseStage.IsTest && !test)
            {
                //User is in a test, page is not a test.
                response.ForceClose = true;
                return Json(response, JsonRequestBehavior.AllowGet);
            }

            UserCourseTestModel curUserCourseTest = null;
            //commented by ilyas
            if (curUserCourse.CourseStage.IsTest)
            {
                curUserCourseTest = curUserCourse.CourseStage.Resource.LatestUserTestFor(curUserCourse.ID);
            }
          
            //Everything is good, beat thy heart!
            Beat(curUserCourse.ID, curUserCourseTest == null ? (int?)null : curUserCourseTest.ID, Convert.ToInt32(curUserCourse.CourseStage.ChapterID) == 0 ? curUserCourse.Course.Resources.FirstOrDefault().Chapters.FirstOrDefault().ID : curUserCourse.CourseStage.ChapterID);

            return Json(response, JsonRequestBehavior.AllowGet);
        }

        [Obsolete]
        private void Beat(int userCourseID, int? testID, int? chapterID)
        {
            var newBeat = new HeartbeatAddModel
            {
                StartOn = DateTime.Now,
                UserCourseID = userCourseID,
                UserCourseTestID = testID,
                ChapterID = chapterID
            };

            Database.ExecuteInsert(newBeat);
        }
        private int AddEditAnswers(string FreeTextans , int questionid , string answerid , int userid)
        {
            try
            {
                if (answerid == "0")
                {
                    TestQuestionAnswerAddModel model = new TestQuestionAnswerAddModel();
                    model.Answer = FreeTextans;
                    model.IsCorrect = true;
                    if (Convert.ToInt32(questionid) > 0)
                    {
                        model.TestQuestionID = Convert.ToInt32(questionid);
                    }
                    var prevAnswers = Database.GetAll<TestQuestionAnswerModel>("WHERE TestQuestionID = @TestQuestionID", new { TestQuestionID = model.TestQuestionID });
                    var lastSort = prevAnswers.Any() ? prevAnswers.Max(x => x.Sort) : 0;
                    model.Sort = lastSort + 1;
                    model.CourseUserId = userid;
                    model.IsResult = 0;
                    int answerId =Database.ExecuteInsert(model,true);
                    return answerId;
                }
            }
            catch(Exception ex)
            {

            }
            return 0;
        }

      //  [HttpPost]
        public JsonResult SubmitFeedback(string UserName, string Email, string Course,string feedback,string CourseId)
        {
            try
            {
                int companyID = new UsersService().GetLoggedInUserModel().Company.ID;
                var company = Database.GetAll<CompanyModel>("WHERE ID = " + companyID).FirstOrDefault();

                var TrainingOfficer = Database.GetAll<SettingsModel>("WHERE CompanyID = " + companyID).FirstOrDefault();



               

                string dbName_SinglePoint = (string)Session["SinglePointDBName"];
                LogApp.Log4Net.WriteLog("sending email for database : " + dbName_SinglePoint, LogApp.LogType.GENERALLOG);

                employeesEntities.Database.Connection.ConnectionString = employeesEntities.Database.Connection.ConnectionString.Replace("DentonsEmployeesForRiskManager", dbName_SinglePoint);

                var modelsmtp = employeesEntities.DentonsEmployeesSettings.FirstOrDefault();

                MailMessage mail = new MailMessage();







                if (TrainingOfficer.TrainingOfficerEmail != "" && TrainingOfficer.TrainingOfficerEmail!=null)
                {
                    mail.To.Add(TrainingOfficer.TrainingOfficerEmail);
                }

                string appemail = ConfigurationManager.AppSettings["ApplicationEmails"];

                LogApp.Log4Net.WriteLog("app Email : " +appemail , LogApp.LogType.GENERALLOG);


                foreach (var address in appemail.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries))
                {
                    mail.To.Add(address);
                }

                                        

                mail.From = new MailAddress(modelsmtp.MailUsername);
             

                mail.Subject = Course+ " - Feedback";
                //mail.Body = "<p> Dear " + TrainingOfficer.TraniningOfficerName + "</p>";



                mail.Body += "<p><b>Course User:</b> " + UserName + "</p>";
                mail.Body += "<p><b>Email:</b> " + Email+ "</p>";

                mail.Body += "<p><b>Company:</b> " + company.Name + "</p>";
                mail.Body += "<p><b>Feedback:</b> " + feedback + "</p>";

                mail.IsBodyHtml = true;
                SmtpClient smtp = new SmtpClient();
                smtp.Host = modelsmtp.MailServer;
                smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                smtp.Port = modelsmtp.SMTPPort;
                smtp.UseDefaultCredentials = false;
                System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;
                smtp.Credentials = new System.Net.NetworkCredential(modelsmtp.MailUsername, modelsmtp.MailPassword); // Enter seders User name and password  
                smtp.EnableSsl = true;
                      smtp.Send(mail);

                var curUser = GetLoggedInUser().ID;
                var usercourse = GetLoggedInUser().UserCourses.Where(x => x.UserID == curUser && x.CourseID ==Convert.ToInt32( CourseId)).FirstOrDefault();

                LogApp.Log4Net.WriteLog("User id : " + curUser + "User Course:"+usercourse.ID, LogApp.LogType.GENERALLOG);


                int ID = usercourse.ID;

                Database.ExecuteUpdate("UserCourses", new[] { "FeedBack" }, new { ID = ID, FeedBack = 1 });


            }
            catch (Exception ex) {
                LogApp.Log4Net.WriteException(ex);

            }

            return Json(new { Feedback = true, Message = "Answer submitted successfuly." }, JsonRequestBehavior.AllowGet);




        }

        private void SendFeedBackEmail()
        {


        }

    }

    public class HeartbeatResponseModel
    {
        public bool Timeout { get; set; }
        public bool ForceClose { get; set; }
    }


    
}
