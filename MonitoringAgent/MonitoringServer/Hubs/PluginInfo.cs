using Microsoft.AspNet.SignalR;

namespace MonitoringServer.Hubs
{
    public class PluginInfo : Hub
    {
        public void SendPluginInfo(/*string pluginName, */string propertyName, object value)
        {
            this.Clients.All.pluginMessage(/*pluginName,*/ propertyName, value);
        }
    }
}