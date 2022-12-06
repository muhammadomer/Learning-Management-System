using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HUC.Web.App.PageModels
{
    public abstract class DashboardPageModel
    {
        public int? Year { get; set; }

        public IEnumerable<int> YearsOptions()
        {
            var list = new List<int>();
            var currentDate = DateTime.Now;
            var oldestYear = new DateTime(2013, 1, 1);
            for (var date = oldestYear; date.Year <= currentDate.Year; date = date.AddYears(1))
            {
                list.Add(date.Year);
            }


            return list.OrderByDescending(x => x);
        }
    }
}