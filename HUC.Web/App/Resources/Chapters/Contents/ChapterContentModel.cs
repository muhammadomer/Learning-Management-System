using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using AtlasDB;
using HUC.Web.App.Shared;

namespace HUC.Web.App.Resources.Chapters.Contents
{
    public class ChapterContentModel : BaseModel
    {
        [Required, Display(Name = "Chapter")]
        public int ChapterID { get; set; }

        [Required, Display(Name = "Sort")]
        public int Sort { get; set; }

        [AllowHtml, Display(Name = "Value")]
        public string Value { get; set; }

        [Required, Display(Name = "Content Template")]
        public ContentType ContentType { get; set; }

        //Lazy
        private ResourceChapterModel _chapter;

        [DBIgnore]
        public ResourceChapterModel Chapter
        {
            get
            {
                if (_chapter == null)
                {
                    _chapter = Database.GetSingle<ResourceChapterModel>(this.ChapterID);
                }
                return _chapter;
            }
            set { _chapter = value; }
        }
    }
}