using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using HUC.Web.App.Shared;

namespace HUC.Web.App.SEOPage
{
    public class SEOPageModel: BaseModel
    {
        public int ID { get; set; }

        [DisplayName("Page Title"), StringLength(500)]
        public string PageTitle { get; set; }

        [DisplayName("Meta Description"), StringLength(500)]
        public string MetaDescription { get; set; }

        [Required, StringLength(500)]
        [DisplayName("URL (Must start with /)")]
        public string URL { get; set; }
    }
}