using Microsoft.AspNet.SignalR;
using Microsoft.Owin;
using MonitoringServer.Controllers;
using Owin;

[assembly: OwinStartup(typeof(MonitoringServer.Startup))]

namespace MonitoringServer
{
    public class Startup
    {   
        public void Configuration(IAppBuilder app)
        {
            var hubConfiguration = new HubConfiguration();
            hubConfiguration.EnableDetailedErrors = true;
            hubConfiguration.EnableJavaScriptProxies = false;
            app.MapSignalR(hubConfiguration);

            SQLiteController.CreateDbFile();
            SQLiteController.CreateTables();

            /*MessageController messageController = new MessageController();
            messageController.StartMessageThread();*/
        }
    }
}