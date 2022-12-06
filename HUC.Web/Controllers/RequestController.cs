using System.Web.Mvc;
using HUC.Web.App.PageModels.Forms;

namespace HUC.Web.Controllers
{
    public class RequestController : BaseController
    {
        //
        // GET: /Site/

        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Index(DemoFormModel model)
        {

            if (ModelState.IsValid)
            {
                new MailController().DemoForm(model).Deliver();

                AddSuccess("Your request for a demo has been sent.");
                return RedirectToAction("Index");
            }

            AddErrorModel();
            return View(model);
        }

        public ActionResult DemoHome()
        {
            return PartialView();
        }

        public ActionResult DemoPod()
        {
            return PartialView();
        }
    }
}
