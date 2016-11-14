using System.Web.Http;
using Microsoft.AspNet.SignalR;
using MonitoringServer.Hubs;
using MonitoringServer.Models;

namespace MonitoringServer.Api
{
    public class PluginController : ApiController
    {
        public void Post(PluginOutput pluginOutput)
        {
            var context = GlobalHost.ConnectionManager.GetHubContext<PluginInfo>();
            context.Clients.All.pluginMessage(/*pluginOutput.PluginName,*/ pluginOutput.PropertyName, pluginOutput.Value);
        }
    }
}
