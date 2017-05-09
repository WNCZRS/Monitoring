using Microsoft.AspNet.SignalR;
using Microsoft.Owin;
using MonitoringServer.Controllers;
using Owin;

//[assembly: OwinStartup(typeof(MonitoringServer.Startup))]

namespace MonitoringServer
{
    public class Startup
    {   
        public void Configuration(IAppBuilder app)
        {
            var hubConfiguration = new HubConfiguration();

            hubConfiguration.EnableDetailedErrors = true;
            hubConfiguration.EnableJavaScriptProxies = true;
            app.MapSignalR(hubConfiguration);

            MessageController.InitDatabase();
            MessageController.StartMessageThread();
        }
    }
}