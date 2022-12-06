using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using HUC.Web.App.SEOPage;

namespace HUC.Web.Areas.Admin.Controllers
{
    public class SEOPagesController : AdminBaseController
    {
        [HttpGet]
        public ActionResult Create(string url = "")
        {
            var retModel = new SEOPageModel {URL = url};

            return View(retModel);
        }

        [HttpPost]
        public ActionResult Create(SEOPageModel model)
        {
            if (!ModelState.IsValid)
            {
                AddError("Please correct the errors below");
                return View(model);
            }

            model.URL = model.URL.ToLower();

            Database.ExecuteInsert(model);

            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Edit(SEOPageModel model)
        {
            if (!ModelState.IsValid)
            {
                AddError("Please correct the errors below");
                return View(model);
            }

            model.URL = model.URL.ToLower();

            Database.ExecuteUpdate(model);

            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult Edit(int id)
        {
            var editModel = Database.GetSingle<SEOPageModel>(id);

            if (editModel == null)
            {
                return RedirectToAction("Index");
            }

            return View(editModel);
        }

        public ActionResult Delete(int id, bool confirm = false)
        {
           
            if (!confirm)
            {
                AddDeleteConfirm("SEO Page", Url.Action("Delete", new { id, confirm = true }));
            }
            else
            {
                Database.HardDelete("SEOPages", id);
                AddSuccess("SEO deleted with success");
            }

            return RedirectToAction("Index");
        }

        public ActionResult Index()
        {
            var all = Database.GetAll<SEOPageModel>("ORDER BY URL DESC");
            return View(all);
        }
    }
}