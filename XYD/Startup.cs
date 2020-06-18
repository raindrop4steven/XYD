using System;
using System.Threading.Tasks;
using Microsoft.Owin;
using Owin;
using Hangfire;
using XYD.Common;

[assembly: OwinStartup(typeof(XYD.Startup))]

namespace XYD
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            // Storage is the only thing required for basic configuration.
            // Just discover what configuration options do you have.
            GlobalConfiguration.Configuration
                .UseSqlServerStorage("DeptOAHangfire");
            app.UseHangfireDashboard();
            app.UseHangfireServer();
            ScheduleUtil.ScheduleGlobalUpdate();
        }
    }
}
