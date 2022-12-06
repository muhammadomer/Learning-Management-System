
using System;
using Microsoft.AspNet.SignalR;
using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(HUC.Web.Startup))]

namespace HUC.Web
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            GlobalHost.Configuration.DisconnectTimeout = TimeSpan.FromSeconds(10);

            GlobalHost.Configuration.KeepAlive = TimeSpan.FromSeconds(3);

            app.MapSignalR();
        }
    }
}
