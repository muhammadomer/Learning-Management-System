using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HUC.Web.Hubs
{
    public class HeartbeatClientResponse
    {
        public bool ForceClose { get; set;}
        public string Message { get; set; }
    }
}