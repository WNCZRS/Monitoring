using Microsoft.Owin;
using MonitoringServer.Controllers;
using Owin;

[assembly: OwinStartup(typeof(MonitoringServer.Startup))]

namespace MonitoringServer
{
    public class Startup
    {
        string _dbName = "D:\\Monitoring\\MonitoringServerDB.sqlite";

        public void Configuration(IAppBuilder app)
        {
            app.MapSignalR();
            SQLiteController.CreateDbFile(_dbName);
            SQLiteController.CreateTables(_dbName);
        }
    }
}