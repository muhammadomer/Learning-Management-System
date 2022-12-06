using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HUC.Web.App.PageModels.Forms
{
    public class ContactFormModel
    {
        [Required, Display(Name = "Your name")]
        public string Name { get; set; }
        [Required, Display(Name = "Email address")]
        public string Email { get; set; }
        [Required, Display(Name = "Your message")]
        public string Message { get; set; }
    }
}