using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Dapper;
using HUC.Web.App.News.Categories;
using HUC.Web.App.News.Items;
using HUC.Web.App.News.Labels;
using HUC.Web.App.Shared;

namespace HUC.Web.Areas.Admin.Controllers
{
    public class NewsController : AdminBaseController
    {
        private const string NoteName = "News Item";

        //
        // GET: /Admin/News/
        public ActionResult Index()
        {
            var model = Database.GetAll<NewsItemModel>("WHERE IsDeleted = 0");

            return View(model);
        }

        public ActionResult Create()
        {
            var model = new NewsItemAddModel();
            return View(model);
        }

        [HttpPost]
        public ActionResult Create(NewsItemAddModel model)
        {
            DateTime parsedPublishOnDate;
            DateTime parsedPublishOnTime;
            var datePublishParseErrors = false;
            if (!DateTime.TryParse(model.PublishOnDateString + " 00:00:00", out parsedPublishOnDate))
            {
                datePublishParseErrors = true;
                ModelState.AddModelError("PublishOnDateString", "Invalid date or format provided.");
            }
            if (!DateTime.TryParse("2000/01/01 " + model.PublishOnTimeString, out parsedPublishOnTime))
            {
                datePublishParseErrors = true;
                ModelState.AddModelError("PublishOnTimeString", "Invalid time or format provided.");
            }
            if (!datePublishParseErrors)
            {
                model.PublishOn = new DateTime(parsedPublishOnDate.Year, parsedPublishOnDate.Month, parsedPublishOnDate.Day, parsedPublishOnTime.Hour, parsedPublishOnTime.Minute, 0);
            }

            var compareTitle = Database.GetAll<NewsItemModel>().SingleOrDefault(x => x.Title.ForUrl() == model.Title.ForUrl());
            if (compareTitle != null)
            {
                ModelState.AddModelError("Title", "The title provided has already been used by another news item.");
            }

            if (ModelState.IsValid)
            {
                var newID = Database.ExecuteInsert(model, true);
                UpdateEnumerables(newID, model.LabelIDs);

                AddSuccessCreate(NoteName);
                return RedirectToAction("Index");
            }

            AddErrorModel();
            return View(model);
        }

        public ActionResult Edit(int id)
        {
            var model = Database.GetSingle<NewsItemEditModel>(id);
            model.LabelIDs = model.Labels.Select(x => x.ID);

            return View(model);
        }

        [HttpPost]
        public ActionResult Edit(NewsItemEditModel model)
        {
            DateTime parsedPublishOnDate;
            DateTime parsedPublishOnTime;
            var datePublishParseErrors = false;
            if (!DateTime.TryParse(model.PublishOnDateString + " 00:00:00", out parsedPublishOnDate))
            {
                datePublishParseErrors = true;
                ModelState.AddModelError("PublishOnDateString", "Invalid date or format provided.");
            }
            if (!DateTime.TryParse("2000/01/01 " + model.PublishOnTimeString, out parsedPublishOnTime))
            {
                datePublishParseErrors = true;
                ModelState.AddModelError("PublishOnTimeString", "Invalid time or format provided.");
            }
            if (!datePublishParseErrors)
            {
                model.PublishOn = new DateTime(parsedPublishOnDate.Year, parsedPublishOnDate.Month, parsedPublishOnDate.Day, parsedPublishOnTime.Hour, parsedPublishOnTime.Minute, 0);
            }

            var compareTitle = Database.GetAll<NewsItemModel>().SingleOrDefault(x => x.Title.ForUrl() == model.Title.ForUrl());
            if (compareTitle != null && compareTitle.ID != model.ID)
            {
                ModelState.AddModelError("Title", "The title provided has already been used by another news item.");
            }

            if (ModelState.IsValid)
            {
                Database.ExecuteUpdate(model);
                UpdateEnumerables(model.ID, model.LabelIDs);

                AddSuccessEdit(NoteName);
                return RedirectToAction("Index");
            }

            AddErrorModel();
            return View(model);
        }

        private void UpdateEnumerables(int id, IEnumerable<int> labels)
        {
            //Delete prev tags
            var sql = "DELETE FROM NewsItemLabels WHERE NewsItemID = @NewsItemID;";
            var sqlParams = new DynamicParameters();
            sqlParams.Add("NewsItemID", id);

            if (labels != null && labels.Any())
            {
                var count = 0;
                foreach (var curLabelID in labels)
                {
                    sql += "INSERT INTO NewsItemLabels(NewsItemID, NewsLabelID) VALUES(@NewsItemID, @NewsLabelID" + count + ");";
                    sqlParams.Add("NewsLabelID" + count, curLabelID);
                    count++;
                }
            }

            Database.Execute(sql, sqlParams);
        }

        public ActionResult Delete(int id, bool undo = false)
        {
            Database.SoftDelete("NewsItems", id, undo);

            if (undo)
            {
                AddDeleteUndone(NoteName);
            }
            else
            {
                AddDeleted(NoteName, Url.Action("Delete", new { id = id, undo = true }));
            }

            return RedirectToAction("Index");
        }

        public ActionResult Categories()
        {
            var model = Database.GetAll<NewsCategoryModel>("WHERE IsDeleted = 0");

            return View(model);
        }

        public ActionResult CategoryCreate()
        {
            return View();
        }

        [HttpPost]
        public ActionResult CategoryCreate(NewsCategoryAddModel model)
        {
            var compareName = Database.GetAll<NewsCategoryModel>().SingleOrDefault(x => x.Name.ForUrl() == model.Name.ForUrl());
            if (compareName != null)
            {
                ModelState.AddModelError("Name", "The name provided has already been used by another category.");
            }

            if (ModelState.IsValid)
            {
                Database.ExecuteInsert(model);

                AddSuccessCreate("News Category");
                return RedirectToAction("Categories");
            }

            AddErrorModel();
            return View(model);
        }

        public ActionResult CategoryEdit(int id)
        {
            var model = Database.GetSingle<NewsCategoryEditModel>(id);

            return View(model);
        }

        [HttpPost]
        public ActionResult CategoryEdit(NewsCategoryEditModel model)
        {
            var compareName = Database.GetAll<NewsCategoryModel>().SingleOrDefault(x => x.Name.ForUrl() == model.Name.ForUrl());
            if (compareName != null && compareName.ID != model.ID)
            {
                ModelState.AddModelError("Name", "The name provided has already been used by another category.");
            }

            if (ModelState.IsValid)
            {
                Database.ExecuteUpdate(model);

                AddSuccessEdit("News Category");
                return RedirectToAction("Categories");
            }

            AddErrorModel();
            return View(model);
        }

        public ActionResult CategoryDelete(int id, bool undo = false)
        {
            Database.SoftDelete("NewsCategories", id, undo);

            if (undo)
            {
                AddDeleteUndone("News Category");
            }
            else
            {
                AddDeleted(NoteName, Url.Action("Delete", new { id = id, undo = true }));
            }

            return RedirectToAction("Index");
        }

        public ActionResult Labels()
        {
            var model = Database.GetAll<NewsLabelModel>("WHERE IsDeleted = 0");

            return View(model);
        }

        public ActionResult LabelCreate()
        {
            return View();
        }

        [HttpPost]
        public ActionResult LabelCreate(NewsLabelAddModel model)
        {
            var compareName = Database.GetAll<NewsLabelModel>().SingleOrDefault(x => x.Name.ForUrl() == model.Name.ForUrl());
            if (compareName != null)
            {
                ModelState.AddModelError("Name", "The name provided has already been used by another label.");
            }

            if (ModelState.IsValid)
            {
                Database.ExecuteInsert(model);

                AddSuccessCreate("News Label");
                return RedirectToAction("Labels");
            }

            AddErrorModel();
            return View(model);
        }

        public ActionResult LabelEdit(int id)
        {
            var model = Database.GetSingle<NewsLabelEditModel>(id);

            return View(model);
        }

        [HttpPost]
        public ActionResult LabelEdit(NewsLabelEditModel model)
        {
            var compareName = Database.GetAll<NewsLabelModel>().SingleOrDefault(x => x.Name.ForUrl() == model.Name.ForUrl());
            if (compareName != null && compareName.ID != model.ID)
            {
                ModelState.AddModelError("Name", "The name provided has already been used by another label.");
            }

            if (ModelState.IsValid)
            {
                Database.ExecuteUpdate(model);

                AddSuccessEdit("News Label");
                return RedirectToAction("Labels");
            }

            AddErrorModel();
            return View(model);
        }

        public ActionResult LabelDelete(int id, bool undo = false)
        {
            Database.SoftDelete("NewsLabels", id, undo);

            if (undo)
            {
                AddDeleteUndone("News Label");
            }
            else
            {
                AddDeleted(NoteName, Url.Action("Delete", new { id = id, undo = true }));
            }

            return RedirectToAction("Index");
        }
    }
}