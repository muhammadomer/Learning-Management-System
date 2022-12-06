
using System;
using System.Data.Metadata.Edm;
using System.Linq;
using System.Threading.Tasks;
using AtlasDB;
using HUC.Web.App.Courses.Time;
using HUC.Web.App.Heartbeats;
using HUC.Web.App.Users;
using HUC.Web.App.Users.Courses;
using Microsoft.AspNet.SignalR.Infrastructure;
using WebGrease.Css.Extensions;
using Connection = Microsoft.AspNet.SignalR.Client.Connection;


namespace HUC.Web.Hubs
{
    public class HeartbeatHub : Microsoft.AspNet.SignalR.Hub<IClient>
    {
        private readonly UsersService _users = new UsersService();

        public override Task OnConnected()
        {
            var connectionId = Context.ConnectionId;

            return base.OnConnected();
        }

        public void TellClient(HeartbeatClientResponse response)
        {
            Clients.Caller.Notify(response);
        }

        /// <summary>
        /// Called in the client side to send data to the server
        /// </summary>
        /// <param name="data"></param>
        public void SendHeartbeatInfo(SignalRHeartbeatModel data)
        {
            var connectionID = Context.ConnectionId;
            var currentUser = _users.GetLoggedInUserModel();
            var response = new HeartbeatClientResponse();
            var db = new AtlasDatabase();

            if (currentUser == null)
            {
                response.ForceClose = true;
                TellClient(response);
            }

            var curUserCourse = currentUser.UserCourses.FirstOrDefault(x => x.CourseID == data.CourseID && x.StartedOn.HasValue);

            if (curUserCourse == null)
            {
                response.ForceClose = true;
                TellClient(response);
                return;
            }

            if (curUserCourse.CourseStage.IsTest && !data.IsTest)
            {
                //User is in a test, page is not a test.
                response.ForceClose = true;
                TellClient(response);
                return;
            }
            // check if there is any other tab open...
            var connections =
                db.Query<HeartbeatModel>(
                    "SELECT * FROM Heartbeats WHERE UserCourseID = @userCourseId AND EndOn is null " +
                    " AND ConnectionID <> @id AND ConnectionID is not null",
                    new {id = connectionID,userCourseId = curUserCourse.ID});

            //If any close them..instead of closing the current one...
            if (connections.Any())
            {
                //kick off the courses
                Clients.Clients(connections.Select(x => x.ConnectionID).ToList()).Notify(new HeartbeatClientResponse { ForceClose = true });
                //TellClient(new HeartbeatClientResponse {ForceClose = true});
            }

            //if (data.IsTest)
            //{
                
            //    // Get all connections currently open in the database
            //    var openConnections =
            //        db.Query<HeartbeatModel>(
            //            "SELECT * FROM Heartbeats WHERE UserCourseID = @userCourseId AND EndOn is null AND ConnectionID <> @id AND ConnectionID is not null",
            //            new { userCourseId = curUserCourse.ID, id = connectionID}).ToList();

                
            //    if (openConnections.Any())
            //    {
            //        Clients.Clients(openConnections.Select(x=>x.ConnectionID).ToList()).Notify(new HeartbeatClientResponse {ForceClose = true});
            //    }
            //}

            UserCourseTestModel curUserCourseTest = null;
            if (curUserCourse.CourseStage.IsTest)
            {
                curUserCourseTest = curUserCourse.CourseStage.Resource.LatestUserTestFor(curUserCourse.ID);
            }

            //Everything is good, beat thy heart!
            Beat(curUserCourse.ID, curUserCourseTest?.ID,
                curUserCourse.CourseStage.ChapterID, connectionID, DateTime.Now, null);

            //Notify client
            
            TellClient(response);
        }

        private void Beat(int userCourseID, int? testID, int? chapterID, string connectionID, DateTime? startOn, DateTime? endOn)
        {
            var db = new AtlasDatabase();

            if (startOn.HasValue)
            {
                var newBeat = new HeartbeatModel
                {
                    UserCourseID = userCourseID,
                    UserCourseTestID = testID,
                    ChapterID = chapterID,
                    ConnectionID = connectionID,
                    StartOn = startOn.Value
                };
                db.ExecuteInsert(newBeat);
            }
            else
            {
                //End Beat
                var oldBeat = db.GetSingle<HeartbeatModel>(connectionID, nameof(connectionID));

                if (oldBeat.EndOn != null) return;// If connection is already closed do NOT close again...

                oldBeat.EndOn = endOn;
                db.ExecuteUpdate(oldBeat);

                // Add to CourseTime to save the time spent in the chapter or test...
                db.ExecuteInsert(new CourseTimeAddModel
                {
                    StartOn = oldBeat.StartOn,
                    EndOn = oldBeat.EndOn.Value,
                    TimeMinutes = (decimal) (oldBeat.EndOn.Value - oldBeat.StartOn).TotalMinutes,
                    UserCourseID = oldBeat.UserCourseID,
                    UserCourseTestID = oldBeat.UserCourseTestID,
                    ChapterID = oldBeat.ChapterID
                });
            }
        }

        public void Disconnect()
        {
            OnDisconnected(true);
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            Console.WriteLine(stopCalled
                ? $"Client {Context.ConnectionId} explicitly closed the connection."
                : $"Client {Context.ConnectionId} timed out .");

            var connectionId = Context.ConnectionId;

            Beat(0, null, null, connectionId, null, DateTime.Now);

            return base.OnDisconnected(stopCalled);
        }

        public override Task OnReconnected()
        {
            var connectionID = Context.ConnectionId;

            return base.OnReconnected();
        }

    }

    public interface IClient
    {
        void Notify(HeartbeatClientResponse response);
    }
}