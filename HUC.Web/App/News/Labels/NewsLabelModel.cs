using System.ComponentModel.DataAnnotations;
using HUC.Web.App.Shared;

namespace HUC.Web.App.News.Labels
{
    public class NewsLabelModel : BaseModel
    {
        [Required, StringLength(500), Display(Name = "Name")]
        public string Name { get; set; }
    }
}