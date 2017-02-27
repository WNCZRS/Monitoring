using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using MonitoringServer.Controllers;
using PluginsCollection;
using System.Threading.Tasks;

namespace MonitoringServer.Hubs
{
    [HubName("MyHub")]
    public class MonitoringHub : Hub
    {
        public bool Send(string message)
        {
            return true;
        }

        public void DoSome(string s)
        {
            Clients.Caller.addMessage(s);
        }

        public void SendPluginOutput(ClientOutput clientOutput)
        {
            if (clientOutput.InitPost)
            {
                //Clients.All.activateTree(clientOutput);
                SQLiteController.SaveBasicInfo(clientOutput);
                MessageController.LoadTreeView();
            }
            else
            {
                SQLiteController.JSONToSQL(clientOutput);
            }
        }

        public void NodeClick(string nodeID, string pcName, string customer)
        {
            MessageController.SetNodeID(nodeID);
            MessageController.SetPCName(pcName);
            MessageController.SetCustomer(customer);
        }

        public void OnSwitchClick()
        {
            MessageController.SwitchView();
        }

        public void OnRefresh()
        {
            MessageController.SetNodeID("");
        }

        public void OnLoadClick()
        {
            MessageController.LoadTreeView();
        }

        public bool CheckConnection(string str)
        {
            return true;
        }

        public override Task OnConnected()
        {                                  
            return base.OnConnected();
        }

        public override Task OnReconnected()
        {
            return base.OnReconnected();
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            return base.OnDisconnected(stopCalled);
        }
    }   
}