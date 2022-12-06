using System.ComponentModel.DataAnnotations;
using HUC.Web.App.Shared;

namespace HUC.Web.App.Testimonials
{
    public class TestimonialModel : BaseModel
    {
        [Required, Display(Name = "Body Copy")]
        public string BodyCopy { get; set; }

        [StringLength(500), Display(Name = "Company Name")]
        public string CompanyName { get; set; }

        [Required, StringLength(500), Display(Name = "Person's Full Name")]
        public string FullName { get; set; }

        [StringLength(500), Display(Name = "Person's Job Title")]
        public string JobTitle { get; set; }

        [Display(Name = "Display on home page?")]
        public bool IsShowOnHomepage { get; set; }

        public int Sort { get; set; }
    }
}