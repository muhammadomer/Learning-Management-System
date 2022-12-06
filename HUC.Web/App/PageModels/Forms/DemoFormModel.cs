using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HUC.Web.App.PageModels.Forms
{
    public class DemoFormModel
    {
        [Required, Display(Name = "Your name")]
        public string Name { get; set; }
        [Required, Display(Name = "Your email address")]
        public string Email { get; set; }
        [Required, Display(Name = "Company name")]
        public string Company { get; set; }
    }
}