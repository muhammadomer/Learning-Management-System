using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using AtlasDB;
using HUC.Web.App.News.Items;
using HUC.Web.App.Shared;

namespace HUC.Web.App.News.Categories
{
    public class NewsCategoryModel : BaseModel
    {
        [Required, StringLength(500), Display(Name = "Name")]
        public string Name { get; set; }

        //Lazy
        private IEnumerable<NewsItemModel> _newsItems;

        [DBIgnore]
        public IEnumerable<NewsItemModel> NewsItems
        {
            get
            {
                if (_newsItems == null)
                {
                    _newsItems = Database.GetAll<NewsItemModel>("WHERE CategoryID = @ID AND IsDeleted = 0", new {ID = this.ID}).Where(x => x.PublishOn <= DateTime.Now);
                }
                return _newsItems;
            }
            set { _newsItems = value; }
        }
    }
}