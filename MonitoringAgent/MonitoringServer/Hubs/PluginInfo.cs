using Microsoft.AspNet.SignalR;
using PluginsCollection;
using System.Collections.Generic;

namespace MonitoringServer.Hubs
{
    public class PluginInfo : Hub
    {
        public void SendPluginInfo(List<PluginOutputCollection> pluginsValues)
        {
            this.Clients.All.pluginMessage(pluginsValues);
        }  
    }
}