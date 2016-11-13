using Microsoft.AspNet.SignalR;

namespace MonitoringServer.Hubs
{
    /*public class CpuInfo : Hub
    {
        public void SendCpuInfo(string machineName, double processor, int memUsage, int totalMemory)
        {
            this.Clients.All.cpuInfoMessage(machineName, processor, memUsage, totalMemory);
        }
    }*/

    public class PluginInfo : Hub
    {
        public void SendPluginInfo(/*string pluginName, */string propertyName, object value)
        {
            this.Clients.All.pluginMessage(/*pluginName,*/ propertyName, value);
        }
    }
}