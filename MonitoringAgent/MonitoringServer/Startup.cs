using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(MonitoringServer.Startup))]

namespace MonitoringServer
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.MapSignalR();
        }
    }
}