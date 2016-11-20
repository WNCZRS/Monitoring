using System.Web.Http;
using Microsoft.AspNet.SignalR;
using MonitoringServer.Hubs;
using MonitoringServer.Models;
using System.Collections.Generic;
using PluginsCollection;

namespace MonitoringServer.Api
{
    public class PluginController : ApiController
    {
        public void Post(List<PluginOutputCollection> pluginOutput)
        {
            var context = GlobalHost.ConnectionManager.GetHubContext<PluginInfo>();
            context.Clients.All.pluginsMessage(pluginOutput);
        }
    }
}
