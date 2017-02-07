using Microsoft.AspNet.SignalR;
using MonitoringServer.Hubs;
using PluginsCollection;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading;
using System.Web;

namespace MonitoringServer.Controllers
{
    public class MessageController
    {
        private bool _threadRunning;

        public string NodeID { get; set; }

        public MessageController()
        {
            _threadRunning = false;
            NodeID = string.Empty;
        }
        public void StartMessageThread()
        {
            Thread pollingThread = null;
            try
            {
                pollingThread = new Thread(new ThreadStart(PluginMessanger));
                if (!_threadRunning)
                {
                    _threadRunning = true;
                    pollingThread.Start();
                }
            }
            catch (Exception)
            {
                _threadRunning = false;
                pollingThread.Abort();
                throw;
            }
        }

        public void PluginMessanger()
        {
            ClientOutput clientOutput;
            DateTime lastPollTime = DateTime.MinValue;
            //var context = GlobalHost.ConnectionManager.GetHubContext<PluginInfo>();

            int threadID = Thread.CurrentThread.ManagedThreadId;
            while (_threadRunning)
            {
                if ((DateTime.Now - lastPollTime).TotalMilliseconds >= 5000)
                {
                    //test only must by on click event (treeNode)
                    clientOutput = SQLiteController.JSONFromSQL("CustomerTest", NodeID, "Marko-PC");

                    if (clientOutput != null)
                    {
                        //context.Clients.All.pluginsMessage(clientOutput);
                    }

                    lastPollTime = DateTime.Now;
                }
                else
                {
                    Thread.Sleep(100);
                }
            }
        }

        public void SetNodeID(string nodeID)
        {
            NodeID = nodeID;
        }
    }
}