using System;
using HUC.Web.App.Shared;

namespace HUC.Web.App.Heartbeats
{
    public class HeartbeatModel : BaseModel
    {
        public string ConnectionID { get; set; }

        public DateTime StartOn { get; set; }

        public DateTime? EndOn { get; set; }
        public int UserCourseID { get; set; }
        public int? UserCourseTestID { get; set; }
        public int? ChapterID { get; set; }

        public DateTime? LastBeatOn { get; set; }
    }
}