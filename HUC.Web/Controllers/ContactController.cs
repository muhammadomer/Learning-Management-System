using System.Web.Mvc;
using BotDetect.Web.UI.Mvc;
using HUC.Web.App.PageModels.Forms;

namespace HUC.Web.Controllers
{
    public class ContactController : BaseController
    {
        //
        // GET: /Site/

        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [CaptchaValidation("CaptchaCode", "SampleCaptcha", "Incorrect CAPTCHA code!")]
        public ActionResult Index(ContactFormModel model)
        {

            if (ModelState.IsValid)
            {
                new MailController().ContactForm(model).Deliver();

                AddSuccess("Your message has been sent.");
                return RedirectToAction("Index");
            }

            AddErrorModel();
            return View(model);
        }
    }
}
