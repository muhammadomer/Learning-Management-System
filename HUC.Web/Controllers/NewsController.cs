using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using HUC.Web.App.News.Categories;
using HUC.Web.App.News.Items;
using HUC.Web.App.PageModels;
using HUC.Web.App.Shared;
using Microsoft.Ajax.Utilities;
using PagedList;

namespace HUC.Web.Controllers
{
    public class NewsController : BaseController
    {
        //
        // GET: /Site/

        public ActionResult Index(int page = 1, string archive = null, string category = null)
        {
            var allItems = Database.GetAll<NewsItemModel>("WHERE IsDeleted = 0").Where(x => x.PublishOn <= DateTime.Now).OrderByDescending(x => x.PublishOn);
            var filteredItems = allItems.AsEnumerable();
            var allCats = Database.GetAll<NewsCategoryModel>();
            var allMonths = allItems.Select(x => x.PublishOn).DistinctBy(x => new DateTime(x.Year, x.Month, 1));

            DateTime? currentArchive = null;
            NewsCategoryModel currentCategory = null;
            if (!String.IsNullOrWhiteSpace(archive))
            {
                DateTime parsedDate;
                if (DateTime.TryParse("1 " + archive, out parsedDate))
                {
                    currentArchive = parsedDate;
                    filteredItems =
                        filteredItems.Where(
                            x => x.PublishOn.Month == parsedDate.Month && x.PublishOn.Year == parsedDate.Year);
                }
            }
            if (!String.IsNullOrWhiteSpace(category))
            {
                currentCategory = allCats.SingleOrDefault(x => x.Name.ForUrl().ToLower() == category.ToLower());
                if (currentCategory != null)
                {
                    filteredItems =
                        filteredItems.Where(x => x.CategoryID.HasValue && x.CategoryID.Value == currentCategory.ID);
                }
            }

            var model = new NewsPageModel
            {
                NewsItems = filteredItems.ToPagedList(page, 5),
                Archives = allMonths,
                Categories = allCats,
                CurrentArchive = currentArchive,
                CurrentCategory = currentCategory
            };

            return View(model);
        }

        public ActionResult Single(string slug)
        {
            var allItems = Database.GetAll<NewsItemModel>();
            var item = allItems.SingleOrDefault(x => x.Title.ForUrl().ToLower() == slug.ToLower());

            if (item == null)
            {
                AddError("Invalid News Item");
                return RedirectToAction("Index");
            }

            var allCats = Database.GetAll<NewsCategoryModel>();
            var allMonths = allItems.Select(x => x.PublishOn).DistinctBy(x => new DateTime(x.Year, x.Month, 1));
            var model = new NewsItemPageModel
            {
                NewsItem = item,
                Archive = allMonths,
                Categories = allCats
            };

            return View(model);
        }

    }
}
