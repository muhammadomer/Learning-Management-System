using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Principal;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.Security;
using AtlasDB;
using HUC.Web.App.Courses.Time;
using HUC.Web.App.Heartbeats;
using HUC.Web.App.Shared;
using HUC.Web.App.Users;
using HUC.Web.Controllers;
using HUC.Web.Hubs;
using LogApp;
using Microsoft.AspNet.SignalR;

namespace HUC.Web
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            //Binder to allow media selector multiselect to work
            //ModelBinders.Binders.DefaultBinder = new CommaSeparatedValuesModelBinder();
            // 

            Log4Net.FilePath = Server.MapPath("./Logs");
            Log4Net.FileName = "Log";
            Log4Net.EnableDBLOG = true;
            Log4Net.EnableERRORLOG = true;
            Log4Net.EnableGENERALLOG = true;
            Log4Net.FileSize = 50;
            Log4Net.TotalFiles = 10;
            Log4Net.Activate(true);
            Log4Net.WriteLog("Application Log initiazlize successfully", LogType.GENERALLOG);


            EndOpenConnections(true);
            new AutomationController().ClientHeatBeatChecker();
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            GlobalConfiguration.Configure(WebApiConfig.Register);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
           // RouteTable.Routes.MapHubs();
        }

        protected void Application_End(object sender, EventArgs e)
        {
            EndOpenConnections(false);
        }

        protected void Application_Error(object sender, EventArgs e)
        {
            EndOpenConnections(false,true);
        }

        private void EndOpenConnections(bool starting, bool isAppError = false)
        {
            //var db = new AtlasDatabase();

            //var now = DateTime.Now;


            //var beats = db.Query<HeartbeatModel>("SELECT * FROM Heartbeats WHERE EndOn is null AND ConnectionID is not null");

            //foreach (var beat in beats)
            //{
            //    db.ExecuteInsert(new CourseTimeAddModel
            //    {
            //        StartOn = beat.StartOn,
            //        EndOn = now,
            //        TimeMinutes = (decimal)(now - beat.StartOn).TotalMinutes,
            //        UserCourseID = beat.UserCourseID,
            //        UserCourseTestID = beat.UserCourseTestID,
            //        ChapterID = beat.ChapterID
            //    });
            //}
            //// End all entries in database missing an EndOn DateTime
            //db.Execute("UPDATE Heartbeats SET EndOn = @now WHERE EndOn is null AND ConnectionID is not null", new { now });

            //Add the time the user spent

            ////File log
            var logFile = System.Web.Hosting.HostingEnvironment.MapPath("/Logs/log.txt");

            if (!File.Exists(logFile))
            {
                File.Create(logFile);
            }

            using (var file = new StreamWriter(logFile, true))
            {
                file.WriteLine(DateTime.Now.ToString("g") + "\t" + (starting ? "starting up" : "going down") + (isAppError?" Application Error":"") +"...");
                file.Close();
            }
        }

        protected void Application_AuthenticateRequest(Object sender, EventArgs e)
        {
            if (System.Web.HttpContext.Current.User != null)
            {
                if (System.Web.HttpContext.Current.User.Identity.IsAuthenticated)
                {
                    if (System.Web.HttpContext.Current.User.Identity is FormsIdentity)
                    {
                        var identity = (FormsIdentity)System.Web.HttpContext.Current.User.Identity;
                        var ticket = identity.Ticket;
                        var data = new AuthModel(ticket.UserData);
                        var roles = new List<string>();

                        foreach (var curRole in data.Roles)
                        {
                            roles.Add(curRole.ToLower());
                        }

                        System.Web.HttpContext.Current.User = new GenericPrincipal(identity, roles.ToArray());
                    }
                }
            }
        }
    }
}
