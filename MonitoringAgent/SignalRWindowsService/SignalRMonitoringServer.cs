using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;
using Microsoft.Owin.Hosting;
using Owin;
using Microsoft.Owin.Cors;
using Microsoft.Owin;
using Microsoft.AspNet.SignalR.Hubs;
using PluginsCollection;
using System.Configuration;

[assembly: OwinStartup(typeof(SignalRWindowsService.Startup))]
namespace SignalRWindowsService
{
    public partial class SignalRMonitoringServer : ServiceBase
    {
        public SignalRMonitoringServer()
        {
            InitializeComponent();
        }         

        protected override void OnStart(string[] args)
        {
            if (!System.Diagnostics.EventLog.SourceExists("SignalRMonitoringServer"))
            {
                System.Diagnostics.EventLog.CreateEventSource(
                    "SignalRMonitoringServer", "Application");
            }
            eventLog1.Source = "SignalRMonitoringServer";
            eventLog1.Log = "Application";

            eventLog1.WriteEntry("In OnStart");
            //string url = "http://localhost:8000";
            string url = ConfigurationManager.AppSettings["ServiecAddress"];
            WebApp.Start(url);
        }

        protected override void OnStop()
        {
            eventLog1.WriteEntry("In OnStop");
        }
    }
    class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            var hubConfiguration = new HubConfiguration();

            hubConfiguration.EnableDetailedErrors = true;
            hubConfiguration.EnableJavaScriptProxies = true;

            app.UseCors(CorsOptions.AllowAll);
            app.MapSignalR(hubConfiguration);

            MessageController.InitDatabase();
            MessageController.StartMessageThread();
        }
    }

    [HubName("MyHub")]
    public class MonitoringHub : Hub
    {
        private static List<HubCallerContext> _connections;
        private static List<string> _users;

        public MonitoringHub()
        {
            _connections = new List<HubCallerContext>();
            _users = new List<string>();
        }

        private void SendCount(int count)
        {
            var context = GlobalHost.ConnectionManager.GetHubContext<MonitoringHub>();
            context.Clients.Group("Clients").UpdateUsersOnlineCount(count);
        }

        public void SendPluginOutput(ClientOutput clientOutput)
        {
            if (clientOutput.InitPost)
            {
                Groups.Add(Context.ConnectionId, "Agents");
                MessageController.SaveBasicInfoToDB(clientOutput);
                MessageController.LoadTreeView();
            }
            else
            {
                MessageController.JSONToSQL(clientOutput);
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
            MessageController.SetNodeID("");
            MessageController.SetView(ViewType.CriticalPreview);
            MessageController.LoadTreeView();
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

            MessageController.SavePosition(computerID, pluginGuid, posTop, posLeft);
        }

        public override Task OnConnected()
        {
            _connections.Add(Context);

            string clientID = GetClientId();

            if (_users.IndexOf(clientID) == -1 && Context.Headers["Cookie"] == null)
            {
                _users.Add(clientID);
                SendCount(_users.Count);
            }
            else
            {
                Groups.Add(Context.ConnectionId, "Clients");
            }
            MessageController.LoadTreeView();

            return base.OnConnected();
        }

        public override Task OnReconnected()
        {
            string clientID = GetClientId();

            if (_users.IndexOf(clientID) == -1)
            {
                _users.Add(clientID);
                SendCount(_users.Count);
            }

            return base.OnReconnected();
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            string clientID = GetClientId();

            if (_users.IndexOf(clientID) > -1)
            {
                _users.Remove(clientID);
                SendCount(_users.Count);
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
