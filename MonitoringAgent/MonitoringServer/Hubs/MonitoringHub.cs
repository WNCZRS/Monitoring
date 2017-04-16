using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using MonitoringServer.Controllers;
using PluginsCollection;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MonitoringServer.Hubs
{
    [HubName("MyHub")]
    public class MonitoringHub : Hub
    {
        public static List<string> Users = new List<string>();
        private static List<HubCallerContext> connections = new List<HubCallerContext>();

        public void SendCount(int count)
        {
            var context = GlobalHost.ConnectionManager.GetHubContext<MonitoringHub>();
            context.Clients.Group("Clients").UpdateUsersOnlineCount(count);
        }

        public void SendPluginOutput(ClientOutput clientOutput)
        {
            if (clientOutput.InitPost)
            {
                Groups.Add(Context.ConnectionId, "Agents");
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
            Clients.Group("Clients").activateTree();
            MessageController.SetNodeID("");
        }

        public void OnLoadClick()
        {           
            MessageController.LoadTreeView();
        }

        public override Task OnConnected()
        {
            connections.Add(Context);

            string clientID = GetClientId();

            if (Users.IndexOf(clientID) == -1 && Context.Headers["Cookie"] == null)
            {
                Users.Add(clientID);
                SendCount(Users.Count);
            }
            else
            {
                Groups.Add(Context.ConnectionId, "Clients");
            }

            return base.OnConnected();
        }

        public override Task OnReconnected()
        {
            string clientID = GetClientId();

            if (Users.IndexOf(clientID) == -1)
            {
                Users.Add(clientID);
                SendCount(Users.Count);
            }

            return base.OnReconnected();
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            string clientID = GetClientId();

            if (Users.IndexOf(clientID) > -1)
            {
                Users.Remove(clientID);
                SendCount(Users.Count);
            }

            return base.OnDisconnected(stopCalled);
        }

        private string GetClientId()
        {
            string clientId = "";
            if (Context.QueryString["clientId"] != null)
            {
                // clientId passed from application 
                clientId = this.Context.QueryString["clientId"];
            }

            if (string.IsNullOrEmpty(clientId.Trim()))
            {
                clientId = Context.ConnectionId;
            }

            return clientId;
        }
    }   
}