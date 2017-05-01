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
        private static List<HubCallerContext> _connections;
        private SQLiteController _sqlController;

        public static List<string> Users = new List<string>();

        public MonitoringHub()
        {
            _connections = new List<HubCallerContext>();
            _sqlController = new SQLiteController();
        }

        public void SendCount(int count)
        {
            var context = GlobalHost.ConnectionManager.GetHubContext<MonitoringHub>();
            context.Clients.Group("Clients").UpdateUsersOnlineCount(count);
        }

        public void SendPluginOutput(ClientOutput clientOutput)
        {
            _sqlController = new SQLiteController();
            if (clientOutput.InitPost)
            {
                Groups.Add(Context.ConnectionId, "Agents");
                _sqlController.SaveBasicInfo(clientOutput);
                MessageController.LoadTreeView();
            }
            else
            {
                _sqlController.JSONToSQL(clientOutput);
            }
        }

        public void NodeClick(string nodeID, string pcName, string group)
        {
            MessageController.SetNodeID(nodeID);
            MessageController.SetPCName(pcName);
            MessageController.SetGroup(group);
            CallOneMachineView();
        }

        public void CallWarningsView()
        {
            MessageController.SetView(ViewType.CriticalPreview);
        }

        public void CallOneMachineView()
        {
            MessageController.SetView(ViewType.OneMachine);
            MessageController.SendSavedPosition();

        }

        public void CallSettingsView()
        {
            MessageController.SetView(ViewType.SettingsView);
            MessageController.SendPluginSettings();
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

        public void SaveHTMLPostion(string computerID, string pluginGuid, int posTop, int posLeft)
        {
            if (string.IsNullOrWhiteSpace(computerID) || string.IsNullOrWhiteSpace(pluginGuid))
            {
                return;                                            
            }

            //var machineID = _sqlController.GetMachineID(computerID);
            SQLiteController s = new SQLiteController();
            s.SaveHTMLPosition(computerID, pluginGuid, posTop, posLeft);
            //_sqlController.SaveHTMLPosition(computerID, pluginGuid, posTop, posLeft);
        }

        public override Task OnConnected()
        {
            _connections.Add(Context);

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