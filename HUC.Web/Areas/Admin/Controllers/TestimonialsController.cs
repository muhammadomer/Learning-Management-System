using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using HUC.Web.App.Testimonials;
using HUC.Web.Models;

namespace HUC.Web.Areas.Admin.Controllers
{
    public class TestimonialsController : AdminBaseController
    {
        private const string NoteName = "Testimonial";

        public ActionResult Index()
        {
            var model = Database.GetAll<TestimonialModel>("WHERE IsDeleted = 0").OrderBy(x => x.Sort);
            return View(model);
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Create(TestimonialAddModel model)
        {

            if (ModelState.IsValid)
            {
                var lastSort = Database.Query<int?>("SELECT MAX(Sort) FROM Testimonials").SingleOrDefault() ?? 0;
                model.Sort = lastSort + 1;

                Database.ExecuteInsert(model);

                AddSuccessCreate(NoteName);
                return RedirectToAction("Index");
            }

            AddErrorModel();
            return View(model);
        }

        public ActionResult Edit(int id)
        {
            var model = Database.GetSingle<TestimonialEditModel>(id);

            return View(model);
        }

        [HttpPost]
        public ActionResult Edit(TestimonialEditModel model)
        {

            if (ModelState.IsValid)
            {
                Database.ExecuteUpdate(model);

                AddSuccessEdit(NoteName);
                return RedirectToAction("Index");
            }

            AddErrorModel();
            return View(model);
        }

        public ActionResult Delete(int id, bool undo = false)
        {
            Database.SoftDelete("Testimonials", id, undo);

            if (undo)
            {
                AddDeleteUndone(NoteName);
            }
            else
            {
                AddDeleted(NoteName, Url.Action("Delete", new { id = id, undo = true }));
            }

            FixSorts();

            return RedirectToAction("Index");
        }

        public ActionResult MoveUp(int id)
        {
            var curItem = Database.GetSingle<TestimonialModel>(id);
            var destSort = curItem.Sort - 1;

            //Up means smaller number (I.E closer to first item)
            Database.Execute(
                "UPDATE Testimonials SET Sort = Sort + 1 WHERE Sort >= @DestSort;" +
                "UPDATE Testimonials SET Sort = @DestSort WHERE ID = @ChangeID;", new {DestSort = destSort, ChangeID = curItem.ID});

            FixSorts();

            AddNotification(NoteType.Success, "Moved Testimonial Up");

            return RedirectToAction("Index");
        }

        public ActionResult MoveDown(int id)
        {
            var curItem = Database.GetSingle<TestimonialModel>(id);
            var destSort = curItem.Sort + 1;

            //Down means larger number (I.E closer to last item)
            Database.Execute(
                "UPDATE Testimonials SET Sort = Sort - 1 WHERE Sort <= @DestSort;" +
                "UPDATE Testimonials SET Sort = @DestSort WHERE ID = @ChangeID;", new { DestSort = destSort, ChangeID = curItem.ID });

            FixSorts();

            AddNotification(NoteType.Success, "Moved Testimonial Down");

            return RedirectToAction("Index");
        }

        private void FixSorts()
        {
            var testimonials = Database.GetAll<TestimonialModel>("WHERE IsDeleted = 0 ORDER BY SORT ASC");

            if (testimonials.Any())
            {
                var updateSQL = "";

                var sort = 1;
                foreach (var curTestimonial in testimonials)
                {
                    updateSQL += "UPDATE Testimonials SET Sort = " + sort + " WHERE ID = " + curTestimonial.ID + ";";

                    sort++;
                }

                Database.Execute(updateSQL);
            }
        }
    }
}
