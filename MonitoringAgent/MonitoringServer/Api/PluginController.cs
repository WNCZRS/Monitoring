using System.Web.Http;
using Microsoft.AspNet.SignalR;
using MonitoringServer.Hubs;
using PluginsCollection;
using System.Data.SQLite;
using System;
using Newtonsoft.Json;
using System.Net;
using MonitoringServer.Controllers;
using System.Threading;

namespace MonitoringServer.Api
{
    public class PluginController : ApiController
    {
        string _dbName = "D:\\Monitoring\\MonitoringServerDB.sqlite";

        [HttpGet]
        public HttpStatusCode Get()
        {
            return HttpStatusCode.OK;
        }

        [HttpPost]
        public void Post(ClientOutput clientOutput)
        {
            var context = GlobalHost.ConnectionManager.GetHubContext<PluginInfo>();

            if (clientOutput.InitPost)
            {
                context.Clients.All.initMessage(clientOutput);
                SQLiteController.SaveBasicInfo(_dbName, clientOutput);
                StartThread();
            }
            SQLiteController.JSONToSQL(_dbName, clientOutput);
        }

       /* [HttpPost]
        [Route("api/Plugin/2")]
        public void PostOnNodeSelected()
        {
            object o = 0;
        }  */

        private void StartThread()
        {
            Thread pollingThread = null;
            try
            {
                pollingThread = new Thread(new ThreadStart(PluginMessanger));
                pollingThread.Start();
            }
            catch (Exception)
            {
                pollingThread.Abort();
                throw;
            }

        }

        private void PluginMessanger()
        {
            DateTime lastPollTime = DateTime.MinValue;

            while (true)
            {
                if ((DateTime.Now - lastPollTime).TotalMilliseconds >= 5000)
                {
                    var context = GlobalHost.ConnectionManager.GetHubContext<PluginInfo>();

                    ClientOutput clientOutput = null;
                    //test only must by on click event (treeNode)
                    clientOutput = SQLiteController.JSONFromSQL(_dbName, "CustomerTest", "C8CBB84172E0", "Marko-PC");

                    context.Clients.All.pluginsMessage(clientOutput);
                    lastPollTime = DateTime.Now;
                }
                else
                {
                    Thread.Sleep(10);
                }
            }
        }

    }
}
