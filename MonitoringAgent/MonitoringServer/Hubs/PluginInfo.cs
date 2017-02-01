using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using MonitoringServer.Controllers;
using PluginsCollection;
using System.Collections.Generic;

namespace MonitoringServer.Hubs
{
    public class PluginInfo : Hub
    {
        MessageController messangerController;

        public void InitMessanger()
        {
            messangerController = new MessageController();
        }

        public void SendPluginInfo(PluginOutput pluginOutput)
        {
            Clients.All.pluginMessage(pluginOutput);
        }

        public void NodeClick(string nodeID)
        {
            messangerController.SetNodeID(nodeID);
        }

        public void onRefresh()
        {
            List<ClientOutput> treeInfo = SQLiteController.GetBasicInfo();
        }
    }

    [HubName("MyHub")]
    public class MonitoringHub : Hub
    {
        public void Send(/*PluginOutput pluginOutput*/ string s)
        {
            //Clients.All.pluginMessage(pluginOutput);
        }
    }   
}