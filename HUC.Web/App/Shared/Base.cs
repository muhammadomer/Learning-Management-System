using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AtlasDB;

namespace HUC.Web.App.Shared
{
    public class BaseModel
    {
        //Shared Fields
        [DBIgnore]
        protected readonly AtlasDatabase Database = new AtlasDatabase();

        public int ID { get; set; }
        //End Shared Fields
    }

    public class BaseService
    {
        protected readonly AtlasDatabase Database = new AtlasDatabase();
    }
}
