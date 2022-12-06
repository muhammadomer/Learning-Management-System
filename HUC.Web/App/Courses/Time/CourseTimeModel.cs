using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AtlasDB;
using HUC.Web.App.Shared;

namespace HUC.Web.App.Courses.Time
{
    public class CourseTimeModel : BaseModel
    {
        public DateTime StartOn { get; set; }
        public DateTime EndOn { get; set; }
       
        public decimal TimeMinutes { get; set; }
        public int UserCourseID { get; set; }
        public int? UserCourseTestID { get; set; }
        public int? ChapterID { get; set; }

        //Lazy
        private decimal? _timeMinutesCalculated;

        [DBIgnore]
        public decimal TimeMinutesCalculated
        {
            get
            {
                if (!_timeMinutesCalculated.HasValue)
                {
                    _timeMinutesCalculated = (decimal)(EndOn - StartOn).TotalMinutes;
                }
                return _timeMinutesCalculated.Value;
            }
            set { _timeMinutesCalculated = value; }
        }
    }
}