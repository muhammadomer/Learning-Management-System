using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using AtlasDB;
using HUC.Web.App.Resources.Chapters.Contents;
using HUC.Web.App.Shared;

namespace HUC.Web.App.Resources.Chapters
{
    public class ResourceChapterModel : BaseModel
    {
        [Required, Display(Name = "Resource")]
        public int ResourceID { get; set; }

        [Required, Display(Name = "Sort")]
        public int Sort { get; set; }

        [Required, StringLength(500), Display(Name = "Name")]
        public string Name { get; set; }

        //Lazy
        private IEnumerable<ChapterContentModel> _contents;
        private ResourceModel _resource;
        private ResourceChapterModel _nextChapter;
        private ResourceChapterModel _prevChapter;

        [DBIgnore]
        public IEnumerable<ChapterContentModel> Contents
        {
            get
            {
                if (_contents == null)
                {
                    _contents = Database.GetAll<ChapterContentModel>("WHERE ChapterID = @ID", new { ID = this.ID });
                }
                return _contents;
            }
            set { _contents = value; }
        }

        [DBIgnore]
        public ResourceModel Resource
        {
            get
            {
                if (_resource == null)
                {
                    _resource = Database.GetSingle<ResourceModel>(this.ResourceID);
                }
                return _resource;
            }
            set { _resource = value; }
        }

        [DBIgnore]
        public ResourceChapterModel NextChapter
        {
            get
            {
                if (_nextChapter == null)
                {
                    _nextChapter = Database.GetAll<ResourceChapterModel>("WHERE ResourceID = @ResourceID AND Sort = @Sort", new { ResourceID = this.ResourceID, Sort = (this.Sort + 1) }).SingleOrDefault();

                    if (_nextChapter == null && this.Resource.NextResource != null && this.Resource.NextResource.Chapters.Any())
                    {
                        _nextChapter = this.Resource.NextResource.Chapters.OrderBy(x => x.Sort).First();
                    }
                }
                return _nextChapter;
            }
            set { _nextChapter = value; }
        }

        [DBIgnore]
        public ResourceChapterModel PrevChapter
        {
            get
            {
                if (_prevChapter == null)
                {
                    _prevChapter = Database.GetAll<ResourceChapterModel>("WHERE ResourceID = @ResourceID AND Sort = @Sort", new { ResourceID = this.ResourceID, Sort = (this.Sort - 1) }).SingleOrDefault();

                    if (_prevChapter == null && this.Resource.PrevResource != null && this.Resource.PrevResource.Chapters.Any())
                    {
                        _prevChapter = this.Resource.PrevResource.Chapters.OrderBy(x => x.Sort).Last();
                    }
                }
                return _prevChapter;
            }
            set { _prevChapter = value; }
        }

        [DBIgnore]
        public bool TestRequired(int userID)
        {
            var isRequired = this.Sort == this.Resource.Chapters.Max(x => x.Sort);
            if (isRequired)
            {
                var prevTest = Database.Query<int>("SELECT COUNT(*) FROM UserCourseTests WHERE ");
            }
            return isRequired;
        }
    }
}