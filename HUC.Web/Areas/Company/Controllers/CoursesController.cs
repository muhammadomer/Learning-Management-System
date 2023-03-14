using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Dapper;
using HUC.Web.App.Courses;
using HUC.Web.App.MediaItems;
using HUC.Web.App.Resources;
using HUC.Web.App.Resources.Chapters;
using HUC.Web.App.Resources.Chapters.Contents;
using HUC.Web.App.Resources.Questions;
using HUC.Web.App.Resources.Questions.Answers;
using HUC.Web.App.Shared;
using HUC.Web.App.Users;
using HUC.Web.App.Users.Courses;
using HUC.Web.SessionExpire;

namespace HUC.Web.Areas.Company.Controllers
{
    [SessionExpireAttribute]
    public class CoursesController : CompanyBaseController
    {
        private const string NoteName = "Course";
        private UsersService _users = new UsersService();
        private FileManipulation _files = new FileManipulation();
        public ActionResult Index()
        {
            int companyId = Convert.ToInt32(HttpContext.Session["CompanyId"]);
            if (companyId == 0)
            {
                var GetLoggedInUser = new UsersService().GetLoggedInUserModel();
                companyId = GetLoggedInUser.Company.ID;
            }
            if(Convert.ToInt32(TempData["NewCourseId"]) > 0)
            {
                TempData["ID"] = Convert.ToInt32(TempData["NewCourseId"]);
            }
            var model = Database.GetAll<CourseModel>("WHERE IsDeleted = 0 and IsCreatedBy = 1 and CompanyId = " + companyId);



          













            return View(model);
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Create(CourseAddModel model)
        {
            var compareName = Database.GetAll<CourseModel>().SingleOrDefault(x => x.Name.ForUrl() == model.Name.ForUrl() && !x.IsDeleted);
            if (compareName != null)
            {
                ModelState.AddModelError("Name", "The name provided has already been used by another course.");
            }
            HttpPostedFileBase BackgroundImage = Request.Files["Backgroundimage"];
            string Backgroundcolor = Request.Form["Backgroundcolor"].ToString();
            if (model.CoolDownHours != null)
            {
                model.ReTake = true;
            }
            else
            {
                model.ReTake = false;

            }
            model.RetakeDuration = model.RetakeDuration == null ? 365 : model.RetakeDuration;
            model.PassingPercentage = model.PassingPercentage == null ? 100 : model.PassingPercentage;
            if (!string.IsNullOrEmpty(BackgroundImage.FileName) && BackgroundImage != null)
            {
                if (_files.UploadImage(BackgroundImage, out string fileName, out string errorMessage))
                {
                    model.Background = fileName;
                    model.BackGroundType = false;
                }
            }
            else if (!string.IsNullOrEmpty(Backgroundcolor))
            {
                model.BackGroundType = true;
                model.Background = Backgroundcolor;
            }
            if (ModelState.IsValid)
            {
                model.CreatedDate = DateTime.Now;
                model.IsCreatedBy = true;
                model.IsVisibleWebsite = true;
                model.IsPublished = false;
                int companyId = Convert.ToInt32(HttpContext.Session["CompanyId"]);
                if(companyId == 0)
                {
                    var GetLoggedInUser = new UsersService().GetLoggedInUserModel();
                    companyId = GetLoggedInUser.Company.ID;
                }
                model.CompanyId = companyId;
                int lastrecord =  Database.ExecuteInsert(model , true);
                //int lastrecord = Database.Query<int>("select Max(ID) from Courses where IsCreatedBy = 1").First();
                TempData["NewCourseId"] = lastrecord;
                AddSuccessCreate(NoteName);
                return RedirectToAction("Index");
            }

            AddErrorModel();
            return View(model);
        }

        public ActionResult Edit(int id)
        {
            var model = Database.GetSingle<CourseEditModel>(id);

            return View(model);
        }

        [HttpPost]
        public ActionResult Edit(CourseEditModel model)
        {

            var checkModel = Database.GetSingle<CourseModel>(model.ID);
            checkModel.Background = "";
            var compareName = Database.GetAll<CourseModel>().SingleOrDefault(x => x.Name.ForUrl() == model.Name.ForUrl() && !x.IsDeleted);
            if (compareName != null && compareName.ID != model.ID)
            {
                ModelState.AddModelError("Name", "The name provided has already been used by another course.");
            }
            HttpPostedFileBase image = Request.Files["Backgroundimage"];
            string Backgroundcolor = Request.Form["Backgroundcolor"].ToString();
            var type = Request.Form["peace"].ToString();


            if (image != null && type.Equals("pic"))
            {
                if (string.IsNullOrEmpty(image.FileName))
                {
                    model.BackGroundType = false;
                }
                else
                {
                    if (!string.IsNullOrEmpty(model.Background))
                    {
                        _files.RemoveImage(model.Background);
                    }
                    if (_files.UploadImage(image, out string fileName, out string errorMessage))
                    {
                        model.Background = fileName;
                        model.BackGroundType = false;
                    }
                }
            }
            else if (!string.IsNullOrEmpty(Backgroundcolor) && type.Equals("col"))
            {
                model.BackGroundType = true;
                model.Background = Backgroundcolor;
            }

            if (model.CoolDownHours != null)
            {
                model.ReTake = true;
            }
            else
            {
                model.ReTake = false;

            }
            model.RetakeDuration = model.RetakeDuration == null ? 365 : model.RetakeDuration;
            model.PassingPercentage = model.PassingPercentage == null ? 100 : model.PassingPercentage;
            if (model.ReTake == true)
            {
                var list = Database.GetAll<UserCourseModel>("where CourseID = CourseID and IsComplete = 1 and CoolDownHoursTime is null", new { CourseID = model.ID });
                if (list.Count() > 0)
                {
                    foreach (var item in list)
                    {
                        // item.CoolDownHoursTime = Convert.ToDateTime(item.CompleteOn).AddHours(model.CoolDownHours == null ? 0 : Convert.ToDouble(model.CoolDownHours));
                        item.CoolDownHoursTime = Convert.ToDateTime(item.CompleteOn).AddMinutes(model.CoolDownHours == null ? 0 : Convert.ToDouble(model.CoolDownHours));
                        Database.ExecuteUpdate(item);
                    }
                }
            }
            if (ModelState.IsValid)
            {
                model.IsPublished = checkModel.IsPublished;
                model.IsCreatedBy = true;
                int companyId = Convert.ToInt32(HttpContext.Session["CompanyId"]);
                model.CompanyId = companyId;
                Database.ExecuteUpdate(model);

                AddSuccessEdit(NoteName);
                return RedirectToAction("Index");
            }

            AddErrorModel();
            return View(model);
        }

        public ActionResult Delete(int id, bool undo = false, bool confirm = false)
        {
            var course = Database.GetSingle<CourseModel>(id);
            if (course.IsPublished && !confirm)
            {
                AddNote("You are about to delete this '" + NoteName + "'. This course is currently published and will be removed from all companies that are using this course. If you would like to continue with this action, please <a href=\"" + Url.Action("Delete", new { id = id, confirm = true }) + "\">click here</a> to confirm.");
            }
            else
            {
                Database.SoftDelete("Courses", id, undo);

                if (undo)
                {
                    AddDeleteUndone(NoteName);
                }
                else
                {
                    AddDeleted(NoteName, Url.Action("Delete", new { id = id, undo = true, confirm = true }));
                }
            }

            return RedirectToAction("Index");
        }
        public ActionResult DeleteWithAjax(int id)
        {
            //var course = Database.GetSingle<CourseModel>(id);


            Database.SoftDelete("Courses", id);


            return Json("yes", JsonRequestBehavior.AllowGet);
        }

        public ActionResult Publish(int id, bool confirm = false)
        {
            var course = Database.GetSingle<CourseModel>(id);
            if (course.IsPublished)
            {
                AddError("This course is already published.");
            }
            if (!course.CanBePublished)
            {
                AddError("This course cannot be published yet.");
            }

            if (confirm)
            {
                var editModel = Database.GetSingle<CourseEditModel>(id);
                editModel.IsPublished = true;
                Database.ExecuteUpdate(editModel);

                //'Compile' the course into course stages!
                course = Database.GetSingle<CourseModel>(id);
                
                var sql = "";
                var sqlParams = new DynamicParameters();
                sqlParams.Add("CourseID", course.ID);

                var stageCount = 1;

                //Course Start
                sql += string.Format("INSERT INTO CourseStages(CourseID, Stage, IsStart) VALUES(@CourseID, {0}, 1);", stageCount);

                stageCount++;
                //

                foreach (var curResource in course.Resources.OrderBy(x => x.Sort))
                {
                    //Chapters
                    int chpid = 0;
                    foreach (var curChapter in curResource.Chapters.OrderBy(x => x.Sort))
                    {
                        sql += string.Format("INSERT INTO CourseStages(CourseID, Stage, ResourceID, ChapterID) VALUES(@CourseID, {0}, @ResourceID{0}, @ChapterID{0});", stageCount);
                        sqlParams.Add("ResourceID" + stageCount, curResource.ID);
                        sqlParams.Add("ChapterID" + stageCount, curChapter.ID);
                        chpid = curChapter.ID;
                        stageCount++;
                    }
                    //

                    if (curResource.Questions.Any())
                    {
                        //Test Start
                        sql += string.Format("INSERT INTO CourseStages(CourseID, Stage, ResourceID, IsTestStart, IsTest) VALUES(@CourseID, {0}, @ResourceID{0}, 1, 1);", stageCount);
                        sqlParams.Add("ResourceID" + stageCount, curResource.ID);



                        stageCount++;
                        //

                        //Test Questions
                        foreach (var curQuestion in curResource.Questions)
                        {
                            sql += string.Format("INSERT INTO CourseStages(CourseID, Stage, ResourceID, TestQuestionID, IsTest) VALUES(@CourseID, {0}, @ResourceID{0}, @TestQuestionID{0}, 1);", stageCount);
                            sqlParams.Add("ResourceID" + stageCount, curResource.ID);
                            sqlParams.Add("TestQuestionID" + stageCount, curQuestion.ID);

                            stageCount++;
                        }
                        //

                        //Test End
                        sql += string.Format("INSERT INTO CourseStages(CourseID, Stage, ResourceID, IsTestEnd, IsTest,ChapterID) VALUES(@CourseID, {0}, @ResourceID{0}, 1, 1, @ChapterID{0});", stageCount);
                        sqlParams.Add("ResourceID" + stageCount, curResource.ID);
                        sqlParams.Add("ChapterID" + stageCount, chpid);
                        stageCount++;
                        //
                    }
                }

                //Course End
                sql += string.Format("INSERT INTO CourseStages(CourseID, Stage, IsEnd) VALUES(@CourseID, {0}, 1);", stageCount);

                stageCount++;
                //
                int currentcompany = Convert.ToInt32(Session["CompanyId"]);
                var GetLoggedInUser = new UsersService().GetLoggedInUserModel();
                sql += "INSERT INTO CompanyCourses(CompanyID, CourseID) VALUES("+ GetLoggedInUser.Company.ID + "," + course.ID + ");";

                Database.Execute(sql, sqlParams);
                //Published to course stages

                AddSuccess("This Course is now published!");
            }
            else
            {
                AddNote("Once you publish a Course, you cannot edit any section of it. <a href=\"" + Url.Action("Publish", new { id = id, confirm = true }) + "\">Click here</a> if you would still like to continue.");
            }

            return RedirectToAction("Index");
        }

        public ActionResult CreateCourseClone(int CourseID, string CourseName)
        {
            List<ContentType> _uploadRequiredContentTypes = new List<ContentType> { ContentType.Audio, ContentType.PDF };
            var oldmodel = Database.GetSingle<CourseModel>(CourseID);
            // var compareName = Database.GetAll<CourseModel>().SingleOrDefault(x => x.Name.ForUrl() == model.Name.ForUrl());
            if (CourseName.Trim().ToLower() == oldmodel.Name.Trim().ToLower() && !oldmodel.IsDeleted)
            {
                AddError("The name provided has already been used by another course.", "Name");

                return RedirectToAction("Index");
            }
            CourseAddModel model = new CourseAddModel();
            model.Name = CourseName;
            model.OutroCopy = oldmodel.OutroCopy;
            model.IsPublished = false;
            model.IntroCopy = oldmodel.IntroCopy;
            model.IsVisibleWebsite = oldmodel.IsVisibleWebsite;
            model.CourseDescription = oldmodel.CourseDescription;
            model.Background = oldmodel.Background;
            model.CreatedDate = DateTime.Now;
            model.IsCreatedBy = true;
            model.ReTake = oldmodel.ReTake;
            model.PassingPercentage = oldmodel.PassingPercentage;
            model.RetakeDuration = oldmodel.RetakeDuration;
            model.CoolDownHours = oldmodel.CoolDownHours;
            int companyId = Convert.ToInt32(HttpContext.Session["CompanyId"]);
            model.CompanyId = companyId;
            if (oldmodel.BackGroundType == false)
            {
                if (!string.IsNullOrEmpty(oldmodel.Background))
                {
                    if (_files.SaveAsNewFile(out string fileName, oldmodel.Background))
                    {
                        model.Background = fileName;
                    }

                }
            }
            else
            {
                model.Background = oldmodel.Background;
            }
            model.BackGroundType = oldmodel.BackGroundType;



            if (ModelState.IsValid)
            {


                Database.ExecuteInsert(model);
                model.ID = Database.Query<CourseModel>("Select top (1) * from Courses order by ID Desc ").FirstOrDefault().ID;
                if (oldmodel.Resources != null && oldmodel.Resources.Count() > 0)
                {
                    List<ResourceAddModel> reslist = new List<ResourceAddModel>();
                    foreach (var item in oldmodel.Resources)
                    {
                        ResourceAddModel resourcemodel = new ResourceAddModel();
                        resourcemodel.CourseID = model.ID;
                        resourcemodel.Sort = item.Sort;
                        resourcemodel.ModuleTime = item.ModuleTime;
                        resourcemodel.Name = item.Name;
                        resourcemodel.TestCooldownHours = item.TestCooldownHours;
                        resourcemodel.TestTimeLimitMinutes = item.TestTimeLimitMinutes;
                        resourcemodel.TestIntroCopy = item.TestIntroCopy;
                        resourcemodel.TestOutroCopy = item.TestOutroCopy;
                        Database.ExecuteInsert(resourcemodel, true);
                        resourcemodel.ID = Database.Query<ResourceModel>("Select top (1) * from Resources order by ID Desc ").FirstOrDefault().ID;
                        reslist.Add(resourcemodel);
                        if (item.Chapters != null && item.Chapters.Count() > 0)
                        {
                            foreach (var chapter in item.Chapters)
                            {

                                ResourceChapterAddModel rspmodel = new ResourceChapterAddModel();
                                rspmodel.ResourceID = resourcemodel.ID;
                                rspmodel.Sort = chapter.Sort;
                                rspmodel.Name = chapter.Name;
                                Database.ExecuteInsert(rspmodel);
                                rspmodel.ID = Database.Query<ResourceChapterModel>("Select top (1) * from ResourceChapters order by ID Desc ").FirstOrDefault().ID;
                                if (chapter.Contents != null && chapter.Contents.Count() > 0)
                                {
                                    foreach (var con in chapter.Contents)
                                    {
                                        ChapterContentAddModel cs = new ChapterContentAddModel();
                                        cs.Sort = con.Sort;
                                        cs.ChapterID = rspmodel.ID;
                                        cs.ContentType = con.ContentType;

                                        if (_uploadRequiredContentTypes.Contains(con.ContentType))
                                        {
                                            if (_files.SaveAsNewFile(out string fileName, con.Value))
                                            {
                                                cs.Value = fileName;
                                            }
                                            else
                                            {
                                                cs.Value = con.Value;
                                            }
                                        }
                                        else
                                        {
                                            cs.Value = con.Value;
                                        }


                                        Database.ExecuteInsert(cs);
                                    }
                                }
                            }

                        }
                        if (item.Questions != null && item.Questions.Count() > 0)
                        {
                            foreach (var ques in item.Questions)
                            {
                                TestQuestionAddModel qs = new TestQuestionAddModel();
                                qs.ResourceID = resourcemodel.ID;
                                qs.Question = ques.Question;
                                qs.QuestionType = ques.QuestionType;
                                qs.Feedback = ques.Feedback;
                                qs.Sort = ques.Sort;
                                qs.Attempts = ques.Attempts;
                                Database.ExecuteInsert(qs);
                                qs.ID = Database.Query<TestQuestionModel>("Select top (1) * from TestQuestions order by ID Desc ").FirstOrDefault().ID;
                                if (ques.Answers != null && ques.Answers.Count() > 0 && ques.QuestionType != 7)  //Not a free text question
                                {
                                    foreach (var ans in ques.Answers)
                                    {
                                        TestQuestionAnswerAddModel ads = new TestQuestionAnswerAddModel();
                                        ads.TestQuestionID = qs.ID;
                                        ads.IsCorrect = ans.IsCorrect;
                                        ads.Answer = ans.Answer;
                                        ads.Sort = ans.Sort;
                                        Database.ExecuteInsert(ads);
                                    }
                                }
                            }
                        }


                    }
                }

                // AddSuccessCreate(NoteName);
                // return RedirectToAction("Index");
            }

            //AddErrorModel();

            return RedirectToAction("Index");
        }

        public ActionResult PreviewCourse(int id)
        {
            var curUserCourse = Database.GetSingle<CourseModel>(id);
            return View(curUserCourse);
        }

        public ActionResult AboutCourse(int id)
        {
            var curUserCourse = Database.GetSingle<CourseModel>(id);
            return View(curUserCourse);
        }


        public ActionResult ModuleDetail(int id)
        {
            var resourceDetail = Database.GetSingle<ResourceModel>(id);
            int lastStageIdByModule = resourceDetail.Course.Stages.Where(x => x.ResourceID == id).OrderByDescending(x => x.Stage).Select(x => x.ID).FirstOrDefault();
            var currentResource = resourceDetail;
            return View(resourceDetail);
        }


        public ActionResult Assessment(int id)
        {
            var curUserCourse = Database.GetSingle<CourseModel>(id);
            return View(curUserCourse);
        }



    }
}