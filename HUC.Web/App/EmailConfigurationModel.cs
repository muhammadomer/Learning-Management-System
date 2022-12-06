using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HUC.Web.App
{
    public class EmailConfigurationModel
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }
        public string Title { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }

        public string Description { get; set; }

    }
}