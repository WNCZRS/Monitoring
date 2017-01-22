using Microsoft.AspNet.SignalR;
using PluginsCollection;
using System.Collections.Generic;

namespace MonitoringServer.Hubs
{
    public class PluginInfo : Hub
    {
        public void SendPluginInfo(PluginOutput pluginOutput)
        {
            this.Clients.All.pluginMessage(pluginOutput);
        }  
    }
}