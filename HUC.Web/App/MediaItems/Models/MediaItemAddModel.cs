using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web;
using AtlasDB;

namespace HUC.Web.App.MediaItems.Models
{
    public class MediaItemAddModel
    {
        [DisplayName("Is this a link?")]
        public bool IsLink { get; set; }
        [Required]
        [StringLength(500)]
        public string Title { get; set; }
        public string Link { get; set; }
        public string RawFileName { get; set; }
        public string DisplayFileName { get; set; }

        public FileType Type { get; set; }

        [DBIgnore]
        public HttpPostedFileBase File { get; set; }

        

    }
}