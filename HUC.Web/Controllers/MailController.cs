using ActionMailer.Net.Mvc;
using HUC.Web.App.PageModels.Forms;
using HUC.Web.App.Users;

namespace HUC.Web.Controllers
{
    public class MailController : MailerBase
    {
        public string AdminEmail = "info@headsupcompliance.co.uk";
        public string NoReply = "no-reply@headsupcompliance.co.uk";

        public EmailResult ContactForm(ContactFormModel model)
        {
            To.Add(AdminEmail);
            From = NoReply;
            Subject = "New Contact Form Submission";
            return Email("ContactForm", model);
        }

        public EmailResult DemoForm(DemoFormModel model)
        {
            To.Add(AdminEmail);
            From = NoReply;
            Subject = "New Demo Request";
            return Email("DemoForm", model);
        }

        public EmailResult Activation(UserModel model)
        {
            To.Add(model.Email);
            From = NoReply;
            Subject = "Activate Your Account";
            return Email("Activation", model);
        }

        public EmailResult ResetPassword(UserModel model)
        {
            To.Add(model.Email);
            From = NoReply;
            Subject = "Reset Your Password";
            return Email("ResetPassword", model);
        }
    }
}
