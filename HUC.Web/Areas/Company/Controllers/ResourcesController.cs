using System;
using System.Collections.Generic;
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
using HUC.Web.Areas.Admin.Controllers;
using HUC.Web.SessionExpire;

namespace HUC.Web.Areas.Company.Controllers
{
    [SessionExpireAttribute]
    public class ResourcesController : CompanyBaseController
    {
        private const string NoteName = "Module";
        private UsersService _users = new UsersService();
        private FileManipulation _files = new FileManipulation();
        private UsersService _usersService;

        public ResourcesController()
        {
            _usersService = new UsersService();
        }
        public ActionResult Index(int id)
        {
            var Course = Database.GetSingle<CourseModel>(id);

            return View(Course);
        }

        public ActionResult Create(int CourseID)
        {
            var model = new ResourceAddModel
            {
                CourseID = CourseID
            };
            if (model.Course.IsPublished)
            {
                AddError("This Course is published.");
                return RedirectToAction("Index", new { id = CourseID });
            }

            return View(model);
        }

        [HttpPost]
        public ActionResult Create(ResourceAddModel model)
        {
            if (model.Course.IsPublished)
            {
                AddError("This Course is published.");
                return RedirectToAction("Index", new { id = model.CourseID });
            }

            if (ModelState.IsValid)
            {
                var prevResources = Database.GetAll<ResourceModel>("WHERE CourseID = @CourseID", new { CourseID = model.CourseID });
                var lastSort = prevResources.Any() ? prevResources.Max(x => x.Sort) : 0;
                model.Sort = lastSort + 1;
                Database.ExecuteInsert(model, true);

                AddSuccessCreate(NoteName);
                return RedirectToAction("Index", new { id = model.CourseID });
            }

            AddErrorModel();
            return View(model);
        }

        public ActionResult Edit(int id)
        {
            var model = Database.GetSingle<ResourceEditModel>(id);
            if (model.Course.IsPublished)
            {
                AddError("This Course is published.");
                return RedirectToAction("Index", new { id = model.CourseID });
            }

            return View(model);
        }

        [HttpPost]
        public ActionResult Edit(ResourceEditModel model)
        {
            if (model.Course.IsPublished)
            {
                AddError("This Course is published.");
                return RedirectToAction("Index", new { id = model.CourseID });
            }

            if (ModelState.IsValid)
            {
                Database.ExecuteUpdate(model);

                AddSuccessEdit(NoteName);
                return RedirectToAction("Index", new { id = model.CourseID });
            }

            AddErrorModel();
            return View(model);
        }

        public ActionResult Delete(int id, bool undo = false)
        {
            var model = Database.GetSingle<ResourceModel>(id);
            if (model.Course.IsPublished)
            {
                AddError("This Course is published.");
                return RedirectToAction("Index", new { id = model.CourseID });
            }

            //  Database.SoftDelete("Resources", id, undo);
            Database.HardDelete("Resources", id);
            var allresource = Database.GetAll<ResourceModel>("WHERE CourseID = @CourseID", new { CourseID = model.CourseID });
            if (allresource != null && allresource.Count() > 0)
            {
                int sort = 1;
                foreach (var item in allresource)
                {
                    item.Sort = sort;
                    Database.ExecuteUpdate(item);
                    sort++;
                }
            }
            if (undo)
            {
                AddDeleteUndone(NoteName);
            }
            else
            {
                AddDeleted(NoteName, Url.Action("Delete", new { id = id, undo = true }));
            }

            return RedirectToAction("Index", new { id = model.CourseID });
        }

        public ActionResult MoveUp(int id)
        {
            //'Up' relates to reducing the sort by one.
            var resource = Database.GetSingle<ResourceModel>(id);
            if (resource.Course.IsPublished)
            {
                AddError("This Course is published.");
                return RedirectToAction("View", new { id = resource.CourseID });
            }
            ChangeResourceSort(resource, resource.Sort - 1);
            FixResourceSorts(resource.CourseID);

            // AddInfo("Updated Sort");
            return RedirectToAction("Index", new { id = resource.CourseID });
        }

        public ActionResult MoveDown(int id)
        {
            //'Down' relates to increasing the sort by one.
            var resource = Database.GetSingle<ResourceModel>(id);
            if (resource.Course.IsPublished)
            {
                AddError("This Course is published.");
                return RedirectToAction("View", new { id = resource.CourseID });
            }
            ChangeResourceSort(resource, resource.Sort + 1);
            FixResourceSorts(resource.CourseID);

            // AddInfo("Updated Sort");
            return RedirectToAction("Index", new { id = resource.CourseID });
        }

        public ActionResult View(int id)
        {
            var model = Database.GetSingle<ResourceModel>(id);
            var curUser = _usersService.GetLoggedInUserModel();
            ViewBag.FullName = curUser.FirstName + " " + curUser.LastName;
            ViewBag.RoleName = "Admin";
            return View(model);
        }

        public ActionResult ChapterEdit(int id)
        {
            var model = Database.GetSingle<ResourceChapterEditModel>(id);

            if (model.Resource.Course.IsPublished)
            {
                AddError("This Course is published.");
                return RedirectToAction("View", new { id = model.ResourceID });
            }

            return View(model);
        }

        [HttpPost]
        public ActionResult ChapterEdit(ResourceChapterEditModel model)
        {

            if (ModelState.IsValid)
            {
                Database.ExecuteUpdate(model);
                FixChapterSorts(model.ResourceID);

                AddSuccessEdit("Chapter");
                return RedirectToAction("View", new { id = model.ResourceID });
            }

            AddErrorModel();
            return View(model);
        }

        public ActionResult ChapterCreate(int id)
        {
            var model = new ResourceChapterAddModel
            {
                ResourceID = id
            };

            if (model.Resource.Course.IsPublished)
            {
                AddError("This Course is published.");
                return RedirectToAction("View", new { id = model.ResourceID });
            }

            return View(model);
        }

        [HttpPost]
        public ActionResult ChapterCreate(ResourceChapterAddModel model)
        {
            if (model.Resource.Course.IsPublished)
            {
                AddError("This Course is published.");
                return RedirectToAction("View", new { id = model.ResourceID });
            }

            if (ModelState.IsValid)
            {
                var prevChapters = Database.GetAll<ResourceChapterModel>("WHERE ResourceID = @ResourceID", new { ResourceID = model.ResourceID });
                var lastSort = prevChapters.Any() ? prevChapters.Max(x => x.Sort) : 0;
                model.Sort = lastSort + 1;
                Database.ExecuteInsert(model);

                AddSuccessCreate("Chapter");
                return RedirectToAction("View", new { id = model.ResourceID });
            }

            AddErrorModel();
            return View(model);
        }

        public ActionResult ChapterMoveUp(int id)
        {
            //'Up' relates to reducing the sort by one.
            var chapter = Database.GetSingle<ResourceChapterModel>(id);
            if (chapter.Resource.Course.IsPublished)
            {
                AddError("This Course is published.");
                return RedirectToAction("View", new { id = chapter.ResourceID });
            }
            ChangeChapterSort(chapter, chapter.Sort - 1);
            FixChapterSorts(chapter.ResourceID);

            //  AddInfo("Updated Sort");
            return RedirectToAction("View", new { id = chapter.ResourceID });
        }

        public ActionResult ChapterMoveDown(int id)
        {
            //'Down' relates to increasing the sort by one.
            var chapter = Database.GetSingle<ResourceChapterModel>(id);
            if (chapter.Resource.Course.IsPublished)
            {
                AddError("This Course is published.");
                return RedirectToAction("View", new { id = chapter.ResourceID });
            }
            ChangeChapterSort(chapter, chapter.Sort + 1);
            FixChapterSorts(chapter.ResourceID);

            //AddInfo("Updated Sort");
            return RedirectToAction("View", new { id = chapter.ResourceID });
        }

        public ActionResult ContentMoveUp(int id)
        {
            //'Up' relates to reducing the sort by one.
            var content = Database.GetSingle<ChapterContentModel>(id);
            ChangeContentSort(content, content.Sort - 1);
            FixContentSorts(content.ChapterID);

            //AddInfo("Updated Sort");
            return RedirectToAction("View", new { id = content.Chapter.ResourceID });
        }

        public ActionResult ContentMoveDown(int id)
        {
            //'Down' relates to increasing the sort by one.
            var content = Database.GetSingle<ChapterContentModel>(id);
            ChangeContentSort(content, content.Sort + 1);
            FixContentSorts(content.ChapterID);

            // AddInfo("Updated Sort");
            return RedirectToAction("View", new { id = content.Chapter.ResourceID });
        }

        public ActionResult ChapterDelete(int id, bool confirm = false)
        {
            var chapter = Database.GetSingle<ResourceChapterModel>(id);
            if (chapter.Resource.Course.IsPublished)
            {
                AddError("This Course is published.");
                return RedirectToAction("View", new { id = chapter.ResourceID });
            }

            if (confirm)
            {
                Database.HardDelete("ResourceChapters", id);
                FixChapterSorts(chapter.ResourceID);
                AddDeleted("Chapter");
            }
            else
            {
                AddDeleteConfirm("Chapter", Url.Action("ChapterDelete", new { id = id, confirm = true }));
            }

            return RedirectToAction("View", new { id = chapter.ResourceID });
        }

        public ActionResult ContentDelete(int id, bool confirm = false)
        {
            var content = Database.GetSingle<ChapterContentModel>(id);

            if (confirm)
            {
                Database.HardDelete("ChapterContents", id);
                FixContentSorts(content.ChapterID);
                AddDeleted("Section");
            }
            else
            {
                AddDeleteConfirm("Section", Url.Action("ContentDelete", new { id = id, confirm = true }));
            }

            return RedirectToAction("View", new { id = content.Chapter.ResourceID });
        }

        List<ContentType> _uploadRequiredContentTypes = new List<ContentType> { ContentType.Audio, ContentType.PDF };

        public ActionResult ContentCreate(int id, int? type = null)
        {
            if (!type.HasValue || !Enum.IsDefined(typeof(ContentType), type.Value))
            {
                var chapter = Database.GetSingle<ResourceChapterModel>(id);
                AddError("Type invalid or not provided.");
                return RedirectToAction("View", new { id = chapter.ResourceID });
            }

            var model = new ChapterContentAddModel
            {
                ChapterID = id,
                ContentType = (ContentType)type.Value
            };

            return View(model);
        }

        [HttpPost]
        public ActionResult ContentCreate(ChapterContentAddModel model, HttpPostedFileBase value_file)
        {
            if (_uploadRequiredContentTypes.Contains(model.ContentType))
            {
                //We require a file to be uploaded (or to already be present)!
                if (value_file != null)
                {
                    //File present, upload!
                    var validFileTypes = new List<FileType> { FileType.Any };
                    switch (model.ContentType)
                    {
                        case ContentType.Audio:
                            validFileTypes = new List<FileType> { FileType.Audio };
                            break;
                        case ContentType.PDF:
                            validFileTypes = new List<FileType> { FileType.PDF };
                            break;
                    }

                    if (!validFileTypes.Contains(_files.GetFileType(value_file)))
                    {
                        ModelState.AddModelError("Value", "Invalid file type provided.");
                    }
                    else
                    {
                        string fileName, errorMessage = null;
                        if (_files.TryUpload(value_file, FileType.Any, out fileName, out errorMessage))
                        {
                            model.Value = fileName;

                            ModelState.Remove("Value");
                        }
                        else
                        {
                            ModelState.AddModelError("Value", errorMessage);
                        }
                    }

                }
                else if (!String.IsNullOrWhiteSpace(model.Value))
                {
                    //No file, but value is present. No errors!
                    ModelState.Remove("Value");
                }
                else
                {
                    //No file, no value, many errors!
                    ModelState.AddModelError("Value", "The " + model.ContentType.StringValue() + " file is required.");
                }
            }
            else
            {
                if (String.IsNullOrWhiteSpace(model.Value))
                {
                    ModelState.AddModelError("Value", "The " + model.ContentType.StringValue() + " field is required.");
                }
            }

            if (ModelState.IsValid)
            {
                var prevContents = Database.GetAll<ChapterContentModel>("WHERE ChapterID = @ChapterID", new { ChapterID = model.ChapterID });
                var lastSort = prevContents.Any() ? prevContents.Max(x => x.Sort) : 0;
                model.Sort = lastSort + 1;
                Database.ExecuteInsert(model);

                AddSuccessCreate("Section");
                return RedirectToAction("View", new { id = model.Chapter.ResourceID });
            }

            AddErrorModel();
            return View(model);
        }

        public ActionResult ContentEdit(int id)
        {
            var model = Database.GetSingle<ChapterContentEditModel>(id);

            return View(model);
        }

        [HttpPost]
        public ActionResult ContentEdit(ChapterContentEditModel model, HttpPostedFileBase value_file)
        {
            if (_uploadRequiredContentTypes.Contains(model.ContentType))
            {
                //We require a file to be uploaded (or to already be present)!
                if (value_file != null)
                {
                    //File present, upload!
                    var validFileTypes = new List<FileType> { FileType.Any };
                    switch (model.ContentType)
                    {
                        case ContentType.Audio:
                            validFileTypes = new List<FileType> { FileType.Audio };
                            break;
                        case ContentType.PDF:
                            validFileTypes = new List<FileType> { FileType.PDF };
                            break;
                    }

                    if (!validFileTypes.Contains(_files.GetFileType(value_file)))
                    {
                        ModelState.AddModelError("Value", "Invalid file type provided.");
                    }
                    else
                    {
                        string fileName, errorMessage = null;
                        if (_files.TryUpload(value_file, FileType.Any, out fileName, out errorMessage))
                        {
                            model.Value = fileName;

                            ModelState.Remove("Value");
                        }
                        else
                        {
                            ModelState.AddModelError("Value", errorMessage);
                        }
                    }

                }
                else if (!String.IsNullOrWhiteSpace(model.Value))
                {
                    //No file, but value is present. No errors!
                    ModelState.Remove("Value");
                }
                else
                {
                    //No file, no value, many errors!
                    ModelState.AddModelError("Value", "The " + model.ContentType.StringValue() + " file is required.");
                }
            }
            else
            {
                if (String.IsNullOrWhiteSpace(model.Value))
                {
                    ModelState.AddModelError("Value", "The " + model.ContentType.StringValue() + " field is required.");
                }
            }

            if (ModelState.IsValid)
            {
                Database.ExecuteUpdate(model);
                FixContentSorts(model.ChapterID);

                AddSuccessEdit("Section");
                return RedirectToAction("View", new { id = model.Chapter.ResourceID });
            }

            AddErrorModel();
            return View(model);
        }

        private void ChangeChapterSort(ResourceChapterModel chapter, int newSort)
        {
            if (newSort < chapter.Sort)
            {
                Database.Execute("UPDATE ResourceChapters SET Sort = Sort + 1 WHERE ResourceID = @ResourceID AND Sort BETWEEN @NewSort AND @OriginalSort - 1;" +
                                 "UPDATE ResourceChapters SET Sort = @NewSort WHERE ID = @ID;", new { ResourceID = chapter.ResourceID, NewSort = newSort, OriginalSort = chapter.Sort, ID = chapter.ID });
            }
            if (newSort > chapter.Sort)
            {
                Database.Execute("UPDATE ResourceChapters SET Sort = Sort - 1 WHERE ResourceID = @ResourceID AND Sort BETWEEN @NewSort AND @OriginalSort + 1;" +
                                 "UPDATE ResourceChapters SET Sort = @NewSort WHERE ID = @ID;", new { ResourceID = chapter.ResourceID, NewSort = newSort, OriginalSort = chapter.Sort, ID = chapter.ID });
            }
        }

        private void ChangeContentSort(ChapterContentModel content, int newSort)
        {
            if (newSort < content.Sort)
            {
                Database.Execute("UPDATE ChapterContents SET Sort = Sort + 1 WHERE ChapterID = @ChapterID AND Sort BETWEEN @NewSort AND @OriginalSort - 1;" +
                                 "UPDATE ChapterContents SET Sort = @NewSort WHERE ID = @ID;", new { ChapterID = content.ChapterID, NewSort = newSort, OriginalSort = content.Sort, ID = content.ID });
            }
            if (newSort > content.Sort)
            {
                Database.Execute("UPDATE ChapterContents SET Sort = Sort - 1 WHERE ChapterID = @ChapterID AND Sort BETWEEN @NewSort AND @OriginalSort + 1;" +
                                 "UPDATE ChapterContents SET Sort = @NewSort WHERE ID = @ID;", new { ChapterID = content.ChapterID, NewSort = newSort, OriginalSort = content.Sort, ID = content.ID });
            }
        }

        private void ChangeQuestionSort(TestQuestionModel question, int newSort)
        {
            if (newSort < question.Sort)
            {
                Database.Execute("UPDATE TestQuestions SET Sort = Sort + 1 WHERE ResourceID = @ResourceID AND Sort BETWEEN @NewSort AND @OriginalSort - 1;" +
                                 "UPDATE TestQuestions SET Sort = @NewSort WHERE ID = @ID;", new { ResourceID = question.ResourceID, NewSort = newSort, OriginalSort = question.Sort, ID = question.ID });
            }
            if (newSort > question.Sort)
            {
                Database.Execute("UPDATE TestQuestions SET Sort = Sort - 1 WHERE ResourceID = @ResourceID AND Sort BETWEEN @NewSort AND @OriginalSort + 1;" +
                                 "UPDATE TestQuestions SET Sort = @NewSort WHERE ID = @ID;", new { ResourceID = question.ResourceID, NewSort = newSort, OriginalSort = question.Sort, ID = question.ID });
            }
        }

        private void ChangeAnswerSort(TestQuestionAnswerModel answer, int newSort)
        {
            if (newSort < answer.Sort)
            {
                Database.Execute("UPDATE TestQuestionAnswers SET Sort = Sort + 1 WHERE TestQuestionID = @TestQuestionID AND Sort BETWEEN @NewSort AND @OriginalSort - 1;" +
                                 "UPDATE TestQuestionAnswers SET Sort = @NewSort WHERE ID = @ID;", new { TestQuestionID = answer.TestQuestionID, NewSort = newSort, OriginalSort = answer.Sort, ID = answer.ID });
            }
            if (newSort > answer.Sort)
            {
                Database.Execute("UPDATE TestQuestionAnswers SET Sort = Sort - 1 WHERE TestQuestionID = @TestQuestionID AND Sort BETWEEN @NewSort AND @OriginalSort + 1;" +
                                 "UPDATE TestQuestionAnswers SET Sort = @NewSort WHERE ID = @ID;", new { TestQuestionID = answer.TestQuestionID, NewSort = newSort, OriginalSort = answer.Sort, ID = answer.ID });
            }
        }

        private void ChangeResourceSort(ResourceModel resource, int newSort)
        {
            if (newSort < resource.Sort)
            {
                Database.Execute("UPDATE Resources SET Sort = Sort + 1 WHERE CourseID = @CourseID AND Sort BETWEEN @NewSort AND @OriginalSort - 1;" +
                                 "UPDATE Resources SET Sort = @NewSort WHERE ID = @ID;", new { CourseID = resource.CourseID, NewSort = newSort, OriginalSort = resource.Sort, ID = resource.ID });
            }
            if (newSort > resource.Sort)
            {
                Database.Execute("UPDATE Resources SET Sort = Sort - 1 WHERE CourseID = @CourseID AND Sort BETWEEN @NewSort AND @OriginalSort + 1;" +
                                 "UPDATE Resources SET Sort = @NewSort WHERE ID = @ID;", new { CourseID = resource.CourseID, NewSort = newSort, OriginalSort = resource.Sort, ID = resource.ID });
            }
        }

        private void FixChapterSorts(int resourceID)
        {
            var resource = Database.GetSingle<ResourceModel>(resourceID);

            if (resource.Chapters.Any())
            {
                var sql = "";
                var sqlParams = new DynamicParameters();

                var count = 1;
                foreach (var chapter in resource.Chapters.OrderBy(x => x.Sort))
                {
                    sql += "UPDATE ResourceChapters SET Sort = @Sort" + count + " WHERE ID = @ID" + count + ";";

                    sqlParams.Add("Sort" + count, count);
                    sqlParams.Add("ID" + count, chapter.ID);
                    count++;
                }

                Database.Execute(sql, sqlParams);
            }
        }

        private void FixContentSorts(int chapterID)
        {
            var chapter = Database.GetSingle<ResourceChapterModel>(chapterID);

            if (chapter.Contents.Any())
            {
                var sql = "";
                var sqlParams = new DynamicParameters();

                var count = 1;
                foreach (var content in chapter.Contents.OrderBy(x => x.Sort))
                {
                    sql += "UPDATE ChapterContents SET Sort = @Sort" + count + " WHERE ID = @ID" + count + ";";

                    sqlParams.Add("Sort" + count, count);
                    sqlParams.Add("ID" + count, content.ID);
                    count++;
                }

                Database.Execute(sql, sqlParams);
            }
        }

        private void FixQuestionSorts(int resourceID)
        {
            var resource = Database.GetSingle<ResourceModel>(resourceID);

            if (resource.Questions.Any())
            {
                var sql = "";
                var sqlParams = new DynamicParameters();

                var count = 1;
                foreach (var chapter in resource.Questions.OrderBy(x => x.Sort))
                {
                    sql += "UPDATE TestQuestions SET Sort = @Sort" + count + " WHERE ID = @ID" + count + ";";

                    sqlParams.Add("Sort" + count, count);
                    sqlParams.Add("ID" + count, chapter.ID);
                    count++;
                }

                Database.Execute(sql, sqlParams);
            }
        }

        private void FixAnswerSorts(int questionID)
        {
            var chapter = Database.GetSingle<TestQuestionModel>(questionID);

            if (chapter.Answers.Any())
            {
                var sql = "";
                var sqlParams = new DynamicParameters();

                var count = 1;
                foreach (var content in chapter.Answers.OrderBy(x => x.Sort))
                {
                    sql += "UPDATE TestQuestionAnswers SET Sort = @Sort" + count + " WHERE ID = @ID" + count + ";";

                    sqlParams.Add("Sort" + count, count);
                    sqlParams.Add("ID" + count, content.ID);
                    count++;
                }

                Database.Execute(sql, sqlParams);
            }
        }

        private void FixResourceSorts(int CourseID)
        {
            var Course = Database.GetSingle<CourseModel>(CourseID);

            if (Course.Resources.Any())
            {
                var sql = "";
                var sqlParams = new DynamicParameters();

                var count = 1;
                foreach (var content in Course.Resources.OrderBy(x => x.Sort))
                {
                    sql += "UPDATE Resources SET Sort = @Sort" + count + " WHERE ID = @ID" + count + ";";

                    sqlParams.Add("Sort" + count, count);
                    sqlParams.Add("ID" + count, content.ID);
                    count++;
                }

                Database.Execute(sql, sqlParams);
            }
        }

        public ActionResult Preview(int id)
        {
            var model = Database.GetSingle<ResourceModel>(id);
            int lastStageIdByModule = model.Course.Stages.Where(x => x.ResourceID == id).OrderByDescending(x => x.Stage).Select(x => x.ID).FirstOrDefault();
            var currentResource = model;
            return View(model);
        }

        public ActionResult Test(int id)
        {
            var model = Database.GetSingle<ResourceModel>(id);

            return View(model);
        }

        public ActionResult QuestionCreate(int id)
        {
            var model = new TestQuestionAddModel
            {
                ResourceID = id
            };

            if (model.Resource.Course.IsPublished)
            {
                AddError("This Course is published.");
                return RedirectToAction("Test", new { id = model.ResourceID });
            }

            return View(model);
        }

        [HttpPost]
        public ActionResult QuestionCreate(TestQuestionAddModel model)
        {
            if (model.Resource.Course.IsPublished)
            {
                AddError("This Course is published.");
                return RedirectToAction("Test", new { id = model.ResourceID });
            }

            if (ModelState.IsValid)
            {
                if (model.Attempts == null || model.Attempts == 0)
                {
                    model.Attempts = 1;
                }
                var prevQuestions = Database.GetAll<TestQuestionModel>("WHERE ResourceID = @ResourceID", new { ResourceID = model.ResourceID });
                var lastSort = prevQuestions.Any() ? prevQuestions.Max(x => x.Sort) : 0;
                model.Sort = lastSort + 1;
                Database.ExecuteInsert(model);

                AddSuccessCreate("Question");

                return RedirectToAction("Test", new { id = model.ResourceID });
            }

            AddErrorModel();
            return View(model);
        }

        public ActionResult QuestionEdit(int id)
        {
            var model = Database.GetSingle<TestQuestionEditModel>(id);
            if (model.Resource.Course.IsPublished)
            {
                AddError("This Course is published.");
                return RedirectToAction("Test", new { id = model.ResourceID });
            }

            return View(model);
        }

        [HttpPost]
        public ActionResult QuestionEdit(TestQuestionEditModel model)
        {
            if (model.Resource.Course.IsPublished)
            {
                AddError("This Course is published.");
                return RedirectToAction("Test", new { id = model.ResourceID });
            }

            if (ModelState.IsValid)
            {
                if (model.Attempts == null)
                {
                    model.Attempts = 1;
                }
                Database.ExecuteUpdate(model);
                FixQuestionSorts(model.ResourceID);

                AddSuccessEdit("Question");
                return RedirectToAction("Test", new { id = model.ResourceID });
            }

            AddErrorModel();
            return View(model);
        }

        public ActionResult QuestionMoveUp(int id)
        {
            //'Up' relates to reducing the sort by one.
            var question = Database.GetSingle<TestQuestionModel>(id);
            if (question.Resource.Course.IsPublished)
            {
                AddError("This Course is published.");
                return RedirectToAction("Test", new { id = question.ResourceID });
            }
            ChangeQuestionSort(question, question.Sort - 1);
            FixQuestionSorts(question.ResourceID);

            // AddInfo("Updated Sort");
            return RedirectToAction("Test", new { id = question.ResourceID });
        }

        public ActionResult QuestionMoveDown(int id)
        {
            //'Down' relates to increasing the sort by one.
            var question = Database.GetSingle<TestQuestionModel>(id);
            if (question.Resource.Course.IsPublished)
            {
                AddError("This Course is published.");
                return RedirectToAction("Test", new { id = question.ResourceID });
            }
            ChangeQuestionSort(question, question.Sort + 1);
            FixQuestionSorts(question.ResourceID);

            //AddInfo("Updated Sort");
            return RedirectToAction("Test", new { id = question.ResourceID });
        }

        public ActionResult QuestionDelete(int id, bool confirm = false)
        {
            var question = Database.GetSingle<TestQuestionModel>(id);
            if (question.Resource.Course.IsPublished)
            {
                AddError("This Course is published.");
                return RedirectToAction("Test", new { id = question.ResourceID });
            }

            if (confirm)
            {
                Database.HardDelete("TestQuestions", id);
                FixQuestionSorts(question.ResourceID);
                AddDeleted("Question");
            }
            else
            {
                AddDeleteConfirm("Question", Url.Action("QuestionDelete", new { id = id, confirm = true }));
            }

            return RedirectToAction("Test", new { id = question.ResourceID });
        }

        public ActionResult AnswerCreate(int id)
        {
            var model = new TestQuestionAnswerAddModel
            {
                TestQuestionID = id
            };
            if (model.Question.Resource.Course.IsPublished)
            {
                AddError("This Course is published.");
                return RedirectToAction("Test", new { id = model.Question.ResourceID });
            }

            return View(model);
        }

        [HttpPost]
        public ActionResult AnswerCreate(TestQuestionAnswerAddModel model)
        {
            if (model.Question.Resource.Course.IsPublished)
            {
                AddError("This Course is published.");
                return RedirectToAction("Test", new { id = model.Question.ResourceID });
            }

            if (ModelState.IsValid)
            {
                var prevAnswers = Database.GetAll<TestQuestionAnswerModel>("WHERE TestQuestionID = @TestQuestionID", new { TestQuestionID = model.TestQuestionID });
                var lastSort = prevAnswers.Any() ? prevAnswers.Max(x => x.Sort) : 0;
                if (model.IsCorrect)
                    Database.Execute("UPDATE TestQuestionAnswers SET IsCorrect = 0 WHERE TestQuestionID = @TestQuestionID", new { TestQuestionID = model.TestQuestionID });
                model.Sort = lastSort + 1;
                Database.ExecuteInsert(model);

                AddSuccessCreate("Answer");

                return RedirectToAction("Test", new { id = model.Question.ResourceID });
            }

            AddErrorModel();
            return View(model);
        }

        public ActionResult AnswerEdit(int id)
        {
            var model = Database.GetSingle<TestQuestionAnswerEditModel>(id);
            if (model.Question.Resource.Course.IsPublished)
            {
                AddError("This Course is published.");
                return RedirectToAction("Test", new { id = model.Question.ResourceID });
            }

            return View(model);
        }

        [HttpPost]
        public ActionResult AnswerEdit(TestQuestionAnswerEditModel model)
        {
            if (model.Question.Resource.Course.IsPublished)
            {
                AddError("This Course is published.");
                return RedirectToAction("Test", new { id = model.Question.ResourceID });
            }

            if (ModelState.IsValid)
            {
                if (model.IsCorrect)
                    Database.Execute("UPDATE TestQuestionAnswers SET IsCorrect = 0 WHERE TestQuestionID = @TestQuestionID", new { TestQuestionID = model.TestQuestionID });
                Database.ExecuteUpdate(model);
            //    FixAnswerSorts(model.TestQuestionID);

                AddSuccessEdit("Answer");
                return RedirectToAction("Test", new { id = model.Question.ResourceID });
            }

            AddErrorModel();
            return View(model);
        }

        public ActionResult AnswerMoveUp(int id)
        {
            //'Up' relates to reducing the sort by one.
            var answer = Database.GetSingle<TestQuestionAnswerModel>(id);
            if (answer.Question.Resource.Course.IsPublished)
            {
                AddError("This Course is published.");
                return RedirectToAction("Test", new { id = answer.Question.ResourceID });
            }
            ChangeAnswerSort(answer, answer.Sort - 1);
          //  FixAnswerSorts(answer.TestQuestionID);

            // AddInfo("Updated Sort");
            return RedirectToAction("Test", new { id = answer.Question.ResourceID });
        }

        public ActionResult AnswerMoveDown(int id)
        {
            //'Down' relates to increasing the sort by one.
            var answer = Database.GetSingle<TestQuestionAnswerModel>(id);
            if (answer.Question.Resource.Course.IsPublished)
            {
                AddError("This Course is published.");
                return RedirectToAction("Test", new { id = answer.Question.ResourceID });
            }
            ChangeAnswerSort(answer, answer.Sort + 1);
            //FixAnswerSorts(answer.TestQuestionID);

            // AddInfo("Updated Sort");
            return RedirectToAction("Test", new { id = answer.Question.ResourceID });
        }

        public ActionResult AnswerDelete(int id, bool confirm = false)
        {
            var answer = Database.GetSingle<TestQuestionAnswerModel>(id);
            if (answer.Question.Resource.Course.IsPublished)
            {
                AddError("This Course is published.");
                return RedirectToAction("Test", new { id = answer.Question.ResourceID });
            }

            if (confirm)
            {
                Database.HardDelete("TestQuestionAnswers", id);
              //  FixAnswerSorts(answer.TestQuestionID);
                AddDeleted("Answer");
            }
            else
            {
                AddDeleteConfirm("Answer", Url.Action("AnswerDelete", new { id = id, confirm = true }));
            }

            return RedirectToAction("Test", new { id = answer.Question.ResourceID });
        }
        [HttpPost]
        public ActionResult UploadFiles()
        {
            // Checking no of files injected in Request object  
            if (Request.Files.Count > 0)
            {
                try
                {
                    string newFileName = "";

                    //  Get all files from Request object  
                    HttpFileCollectionBase files = Request.Files;
                    for (int i = 0; i < files.Count; i++)
                    {
                        //string path = AppDomain.CurrentDomain.BaseDirectory + "Uploads/";  
                        //string filename = Path.GetFileName(Request.Files[i].FileName);  

                        HttpPostedFileBase file = files[i];
                        string fname;

                        // Checking for Internet Explorer  
                        if (Request.Browser.Browser.ToUpper() == "IE" || Request.Browser.Browser.ToUpper() == "INTERNETEXPLORER")
                        {
                            string[] testfiles = file.FileName.Split(new char[] { '\\' });
                            fname = testfiles[testfiles.Length - 1];
                        }
                        else
                        {
                            fname = file.FileName;
                        }
                        if (!Directory.Exists(Server.MapPath("~/_Content/images/flip")))
                        {
                            Directory.CreateDirectory(Server.MapPath("~/_Content/images/flip"));
                        }
                        string extension = Path.GetExtension(fname);
                        newFileName = Guid.NewGuid() + extension;
                        // Get the complete folder path and store the file inside it.  
                        fname = Path.Combine(Server.MapPath("~/_Content/images/flip"), newFileName);
                        file.SaveAs(fname);
                    }
                    // Returns message that successfully uploaded  
                    return Json(new { Message = "1", Data = "~/_Content/images/flip/" + newFileName });
                }
                catch (Exception ex)
                {
                    return Json(new { Message = "Error occurred. Error details: " + ex.Message, Data = "" });
                }
            }
            else
            {
                return Json("No files selected.");
            }
        }
        public ActionResult Question(int id, int check)
        {


            if (check.Equals(0))
            {
                var model = new TestQuestionAddModel
                {
                    ResourceID = id,
                    ID = 0
                };
                if (model.Resource.Course.IsPublished)
                {
                    AddError("This Course is published.");
                    return RedirectToAction("Test", new { id = model.ResourceID });
                }
                return PartialView("_QuestionCompany", model);
            }
            else
            {
                var editmodel = Database.GetSingle<TestQuestionEditModel>(id);
                ViewBag.Answers = Database.GetAll<TestQuestionAnswerEditModel>("where TestQuestionId = @QuestionID", new { QuestionID = id });
                if (editmodel.Resource.Course.IsPublished)
                {
                    AddError("This Course is published.");
                    return RedirectToAction("Test", new { id = editmodel.ResourceID });
                }
                return PartialView("_QuestionCompany", editmodel);

            }


        }
        [HttpPost]
        public JsonResult AddEditQuestion(TestQuestionAddModel model)
        {
            string val = "0";
            if (model.Resource.Course.IsPublished)
            {
                AddError("This Course is published.");
                return Json(val, JsonRequestBehavior.AllowGet);
                // return RedirectToAction("Test", new { id = model.ResourceID });
            }
            if (model.ID == 0 || model.ID == model.ResourceID)
            {
                if (model.Attempts == null || model.Attempts == 0)
                {
                    model.Attempts = 1;
                }
                var prevQuestions = Database.GetAll<TestQuestionModel>("WHERE ResourceID = @ResourceID", new { ResourceID = model.ResourceID });
                var lastSort = prevQuestions.Any() ? prevQuestions.Max(x => x.Sort) : 0;
                model.Sort = lastSort + 1;
                Database.ExecuteInsert(model);

                //  AddSuccessCreate("Question");
            }
            else
            {
                if (model.Attempts == null)
                {
                    model.Attempts = 1;
                }
                Database.ExecuteUpdate(model);
                FixQuestionSorts(model.ResourceID);

                // AddSuccessEdit("Question");
                //return RedirectToAction("Test", new { id = model.ResourceID });
            }
            val = "1";
            return Json(val, JsonRequestBehavior.AllowGet);
        }
        public JsonResult AddEditAnswers(IEnumerable<TestQuestionAnswerModelForJson> alist)
        {
            if (alist.Count() > 0)
            {
                foreach (var item in alist)
                {
                    if (item.answerid == "0")
                    {
                        LogApp.Log4Net.WriteLog("answerid:" + 0, LogApp.LogType.GENERALLOG);

                        TestQuestionAnswerAddModel model = new TestQuestionAnswerAddModel();
                        model.Answer = item.option;
                        model.IsCorrect = Convert.ToInt32(item.selected) == 1 ? true : false;
                        if (Convert.ToInt32(item.questionid) > 0)
                        {
                            model.TestQuestionID = Convert.ToInt32(item.questionid);

                        }
                        else
                        {
                            model.TestQuestionID = Database.Query<TestQuestionModel>("Select top(1) * from TestQuestions order by ID desc").FirstOrDefault().ID;
                        }

                        var prevAnswers = Database.GetAll<TestQuestionAnswerModel>("WHERE TestQuestionID = @TestQuestionID", new { TestQuestionID = model.TestQuestionID });
                        var lastSort = prevAnswers.Any() ? prevAnswers.Max(x => x.Sort) : 0;
                        if (Convert.ToInt32(item.type) == 4 || Convert.ToInt32(item.type) == 6)
                        {

                        }
                        else
                        {

                            if (model.IsCorrect)
                                Database.Execute("UPDATE TestQuestionAnswers SET IsCorrect = 0 WHERE TestQuestionID = @TestQuestionID", new { TestQuestionID = model.TestQuestionID });
                        }
                        model.Sort = lastSort + 1;
                        Database.ExecuteInsert(model);

                        // AddSuccessCreate("Answer");

                        ///  return RedirectToAction("Test", new { id = model.Question.ResourceID });

                    }
                    else
                    {

                        LogApp.Log4Net.WriteLog("answerid:" + item.answerid, LogApp.LogType.GENERALLOG);

                        TestQuestionAnswerEditModel model = new TestQuestionAnswerEditModel();
                        model.ID = Convert.ToInt32(item.answerid);
                        model.Answer = item.option;
                        model.IsCorrect = Convert.ToInt32(item.selected) == 1 ? true : false;
                        model.TestQuestionID = Convert.ToInt32(item.questionid);
                        if (Convert.ToInt32(item.type) == 4 || Convert.ToInt32(item.type) == 6)
                        {

                        }
                        else
                        {
                            if (model.IsCorrect)
                                Database.Execute("UPDATE TestQuestionAnswers SET IsCorrect = 0 WHERE TestQuestionID = @TestQuestionID", new { TestQuestionID = model.TestQuestionID });
                        }
                        Database.ExecuteUpdate(model);
                      //  FixAnswerSorts(model.TestQuestionID);

                        // AddSuccessEdit("Answer");
                        //return RedirectToAction("Test", new { id = model.Question.ResourceID });

                    }
                }

            }
            AddSuccessCreate("Question");
            return Json(HttpContext.Request.ApplicationPath.Length > 1 ? HttpContext.Request.ApplicationPath : string.Empty, JsonRequestBehavior.AllowGet);
        }
        public ActionResult QuestionPartial(int id)
        {
            var model = new TestQuestionAddModel
            {
                ResourceID = id
            };

            if (model.Resource.Course.IsPublished)
            {
                AddError("This Course is published.");
                return RedirectToAction("Test", new { id = model.ResourceID });
            }
            return PartialView("QuestionPartial", model);
        }

    }
}