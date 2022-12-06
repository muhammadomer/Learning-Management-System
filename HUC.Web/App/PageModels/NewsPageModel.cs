using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using HUC.Web.App.News.Categories;
using HUC.Web.App.News.Items;
using PagedList;

namespace HUC.Web.App.PageModels
{
    public class NewsPageModel
    {
        public IPagedList<NewsItemModel> NewsItems { get; set; }
        public IEnumerable<DateTime> Archives { get; set; }
        public IEnumerable<NewsCategoryModel> Categories { get; set; }

        public DateTime? CurrentArchive { get; set; }
        public NewsCategoryModel CurrentCategory { get; set; }
    }
}