using System.Web.Http;
using Microsoft.AspNet.SignalR;
using MonitoringServer.Hubs;
using MonitoringServer.Models;

namespace MonitoringServer.Api
{
    public class PluginController : ApiController
    {
        /*public void Post(CpuInfoPostData cpuInfo)
        {
            var context = GlobalHost.ConnectionManager.GetHubContext<CpuInfo>();
            context.Clients.All.cpuInfoMessage(cpuInfo.MachineName, cpuInfo.Processor, cpuInfo.MemUsage, cpuInfo.TotalMemory);
        }*/
        public void Post(PluginOutput pluginOutput)
        {
            var context = GlobalHost.ConnectionManager.GetHubContext<PluginInfo>();
            context.Clients.All.pluginMessage(/*pluginOutput.PluginName,*/ pluginOutput.PropertyName, pluginOutput.Value);
        }
    }
}
