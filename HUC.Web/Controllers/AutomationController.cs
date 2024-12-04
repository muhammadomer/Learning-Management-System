using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Timers;
using System.Web;
using System.Web.Mvc;
using HUC.Web.App.Courses.Time;
using HUC.Web.App.Heartbeats;
using HUC.Web.App.Shared;
using HUC.Web.App.Users.Courses;

namespace HUC.Web.Controllers
{
    public class AutomationController : BaseController
    {
        //private readonly List<object> _heartbeatChunks = new List<object>();
        //private const int HeartbeatMinuteStep = 5;

        public void ClientHeatBeatChecker()
        {
            var aTimer = new System.Timers.Timer(20000);
            // Hook up the Elapsed event for the timer. 
            aTimer.Elapsed += Checker;
            aTimer.AutoReset = true;
            aTimer.Enabled = true;
        }


        private void Checker(object source, ElapsedEventArgs e)
        {
            try
            {
                var now = DateTime.Now;

                var time = DateTime.Now.AddSeconds(-10);
                var connections =
                     Database.Query<HeartbeatModel>("SELECT * FROM Heartbeats WHERE EndOn is null AND LastBeatOn < @now",
                         new { now = time });

                //Add the time spent...
                foreach (var beat in connections)
                {
                    Database.ExecuteInsert(new CourseTimeAddModel
                    {
                        StartOn = beat.StartOn,
                        EndOn = now,
                        TimeMinutes = (decimal)(now - beat.StartOn).TotalMinutes,
                        UserCourseID = beat.UserCourseID,
                        UserCourseTestID = beat.UserCourseTestID,
                        ChapterID = beat.ChapterID
                    });
                }
                Database.Execute("UPDATE Heartbeats SET EndOn = @date WHERE EndOn is null AND LastBeatOn < @now",
                       new { now = time, date = now });
            }
            catch (Exception ex)
            {

            }
        }

        public ActionResult ClientAliveMessage(string connectionId)
        {
            var beat = Database.GetSingle<HeartbeatModel>(connectionId, "ConnectionID");

            if (beat == null) return Json(true, JsonRequestBehavior.AllowGet);

            beat.LastBeatOn = DateTime.Now;
            Database.ExecuteUpdate(beat);

            return Json(true, JsonRequestBehavior.AllowGet);
        }

        //public JsonResult Index(string svrCode)
        //{
        //    if (svrCode != "A7FB7588-567C-4855-A9F3-26F44EB1CA8D")
        //    {
        //        throw new HttpException(404, null);
        //    }
        //    else
        //    {
        //        //get all heartbeats
        //        var heartbeatsGrouped = Database.GetAll<HeartbeatModel>("WHERE IsConsolidated = 0").GroupBy(x => x.UserCourseID);

        //        foreach (var userCourseBeatGroup in heartbeatsGrouped)
        //        {
        //            var userCourseID = userCourseBeatGroup.Key;
        //            HeartbeatModel prevHeartbeat = null;
        //            HeartbeatModel heartbeatChunkEnd = null;
        //            int? testID = null;
        //            int? chapterID = null;

        //            //We order the dates of the beats so they are in descending order.
        //            var orderedBeats = userCourseBeatGroup.OrderByDescending(x => x.StartOn);

        //            foreach (var curHeartbeat in orderedBeats)
        //            {
        //                if (prevHeartbeat == null)
        //                {
        //                    //prev heartbeat being null signifies this is the first item.
        //                    testID = curHeartbeat.UserCourseTestID;
        //                    chapterID = curHeartbeat.ChapterID;
        //                    heartbeatChunkEnd = curHeartbeat;
        //                }
        //                else
        //                {
        //                    //Check time difference for the 5 minute gap
        //                    if (curHeartbeat.StartOn.AddMinutes(HeartbeatMinuteStep) < prevHeartbeat.StartOn)
        //                    {
        //                        //Time difference is 5 minutes or more. Prev heartbeat becomes the start of chunk. Save this chunk!

        //                        HeartbeatChunkSave(userCourseID, prevHeartbeat, heartbeatChunkEnd, testID, chapterID);
        //                        heartbeatChunkEnd = curHeartbeat;
        //                    }
        //                    else
        //                    {
        //                        if (testID != curHeartbeat.UserCourseTestID || chapterID != curHeartbeat.ChapterID)
        //                        {
        //                            HeartbeatChunkSave(userCourseID, prevHeartbeat, heartbeatChunkEnd, testID, chapterID);

        //                            testID = curHeartbeat.UserCourseTestID;
        //                            chapterID = curHeartbeat.ChapterID;
        //                            heartbeatChunkEnd = curHeartbeat;
        //                        }
        //                        //Current heartbeat is still within range, do nothing
        //                    }
        //                }

        //                prevHeartbeat = curHeartbeat;

        //                if (curHeartbeat.ID == orderedBeats.Last().ID)
        //                {
        //                    //This is last item, do some final checks!
        //                    HeartbeatChunkSave(userCourseID, curHeartbeat, heartbeatChunkEnd, prevHeartbeat.UserCourseTestID, prevHeartbeat.ChapterID);
        //                }
        //            }
        //        }


        //        return Json(_heartbeatChunks.ToArray(), JsonRequestBehavior.AllowGet);
        //    }
        //}


        //        public void HeartbeatChunkSave(int userCourseID, HeartbeatModel start, HeartbeatModel end, int? testID = null, int? chapterID = null)
        //        {
        ////            var isAlive = end.StartOn.AddMinutes((HeartbeatMinuteStep * 2)) > DateTime.Now; //Give ourself double the normal beat step to allow for errors to be avoided
        //            var isAlive = false; //Only enable this during dev for testing as you can control exactly when heartbeats are consolidated

        //            _heartbeatChunks.Add(new
        //            {
        //                Alive = isAlive,
        //                UCID = userCourseID,
        //                Start = start.StartOn.ToString(),
        //                End = end.StartOn.ToString(),
        //                TestID = testID,
        //                ChapterID = chapterID
        //            });

        //            if (!isAlive)
        //            {
        //                //We want to save this range. First we delete the range from the database, then save it into the timings database
        //                Database.Execute("UPDATE Heartbeats SET IsConsolidated = 1, ConsolidatedOn = @Now WHERE StartOn >= @Start AND StartOn <= @End AND UserCourseID = @UserCourseID AND UserCourseTestID " + (testID.HasValue ? "= @UserCourseTestID" : "IS NULL") + " AND ChapterID " + (chapterID.HasValue ? "= @ChapterID" : "IS NULL"),
        //                    new { Start = start.StartOn, End = end.StartOn, UserCourseTestID = testID, ChapterID = chapterID, UserCourseID = userCourseID, Now = DateTime.Now });

        //                Database.ExecuteInsert(new CourseTimeAddModel
        //                {
        //                    StartOn = start.StartOn,
        //                    EndOn = end.StartOn,
        //                    TimeMinutes = (decimal)(end.StartOn - start.StartOn).TotalMinutes,
        //                    UserCourseID = userCourseID,
        //                    UserCourseTestID = testID,
        //                    ChapterID = chapterID
        //                });
        //            }
        //        }
    }
}
