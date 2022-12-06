using System;
using System.Linq;
using System.Web.Mvc;
using HUC.Web.App.Testimonials;

namespace HUC.Web.Controllers
{
    public class SiteController : BaseController
    {
        //
        // GET: /Site/

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Footer()
        {
            return PartialView();
        }

        public ActionResult IndexTestimonial()
        {
            var model = Database.GetAll<TestimonialModel>("WHERE IsDeleted = 0 AND IsShowOnHomepage = 1").OrderBy(x => Guid.NewGuid()).FirstOrDefault();

            return PartialView(model);
        }
    }
}
