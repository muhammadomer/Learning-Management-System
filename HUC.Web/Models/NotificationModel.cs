using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HUC.Web.Models
{
    public class NotificationModel
    {
        public NoteType Type { get; set; }

        public string Message { get; set; }
        public string Title { get; set; }
    }

    public enum NoteType
    {
        Error,
        Info,
        Success,
        Note
    }
}
