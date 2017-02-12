using Microsoft.AspNet.SignalR;
using MonitoringServer.Hubs;
using PluginsCollection;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MonitoringServer.Controllers
{
    enum ViewType
    {
        OneMachine,
        CriticalPreview
    }
    public class MessageController
    {
        private static string _nodeID;
        private static string _customer;
        private static string _pcName;
        private static bool _nodeChanged;
        private static ViewType _viewType;

        private MessageController()
        {
            _customer = string.Empty;
            _nodeID = string.Empty;
            _pcName = string.Empty;
            _nodeChanged = false;
            _viewType = ViewType.OneMachine;
        }
        public static void StartMessageThread()
        {
            Thread pollingThread = null;
            try
            {
                pollingThread = new Thread(new ThreadStart(Messanger));
                pollingThread.Start();
            }
            catch (Exception ex )
            {
                _nodeChanged = false;
                pollingThread.Abort();
                throw ex;
            }
        }

        public static void Messanger()
        {
            DateTime lastPollTime = DateTime.MinValue;

            int threadID = Thread.CurrentThread.ManagedThreadId;
            while(true)
            {
                if (((DateTime.Now - lastPollTime).TotalMilliseconds >= 5000) || _nodeChanged)
                {
                    _nodeChanged = false;
                    switch (_viewType)
                    {
                        case ViewType.OneMachine:
                            OneMachineView();
                            break;

                        case ViewType.CriticalPreview:
                            CriticalPreview();
                            break;

                        default:
                            break;
                    }
                    lastPollTime = DateTime.Now;
                }
                else
                {
                    Thread.Sleep(100);
                }
            }
        }

        private static void CriticalPreview()
        {
            try
            {
                List<ClientOutput> clientOutputList = SQLiteController.CriticalValuesFromDB();
                if (clientOutputList != null || clientOutputList.Count != 0)
                {
                    foreach (ClientOutput co in clientOutputList)
                    {
                        foreach (PluginOutputCollection pluginCollection in co.CollectionList)
                        {
                            foreach (PluginOutput pluginOutput in pluginCollection.PluginOutputList)
                            {
                                List<SimplePluginOutput> criticalValues = pluginOutput.Values.FindAll(item => item.IsCritical == true);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private static void OneMachineView()
        {
            try
            {
                ClientOutput clientOutput = SQLiteController.JSONFromSQL(_customer, _nodeID, _pcName);
                if (clientOutput != null)
                {
                    GetContext().Clients.All.pluginsMessage(clientOutput);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private static IHubContext GetContext()
        {
            return GlobalHost.ConnectionManager.GetHubContext<MonitoringHub>();
        }

        public static void SetNodeID(string nodeID)
        {
            _nodeID = nodeID;
            _nodeChanged = true;
        }

        public static void SetPCName(string pcName)
        {
            _pcName = pcName;
            _nodeChanged = true;
        }

        public static void SetCustomer(string customer)
        {
            _customer = customer;
            _nodeChanged = true;
        }
    }
}