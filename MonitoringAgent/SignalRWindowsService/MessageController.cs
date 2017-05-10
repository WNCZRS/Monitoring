using Microsoft.AspNet.SignalR;
//using MonitoringServer.Hubs;
using PluginsCollection;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SignalRWindowsService
{
    public enum ViewType
    {
        OneMachine,
        CriticalPreview,
        SettingsView
    }

    public class MessageController
    {
        private static string _nodeID;
        private static string _group;
        private static string _pcName;
        private static bool _changed;
        private static ViewType _viewType;

        //public ViewType ViewType
        //{
        //    get
        //    {
        //        return _viewType;
        //    }
        //    set
        //    {
        //        _viewType = value;
        //    }
        //}

        public MessageController()
        {
            _group = string.Empty;
            _nodeID = string.Empty;
            _pcName = string.Empty;
            _changed = false;
            _viewType = ViewType.CriticalPreview;
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
                _changed = false;
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
                if (((DateTime.Now - lastPollTime).TotalMilliseconds >= 5000) || _changed)
                {
                    _changed = false;
                    switch (_viewType)
                    {
                        case ViewType.OneMachine:
                            OneMachineView();
                            break;

                        case ViewType.CriticalPreview:
                            CriticalPreview();
                            break;

                        case ViewType.SettingsView:
                            //nothing to sending periodically
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
            //GetContext().Clients.Group("Clients").InitMainDiv();
            try
            {
                SQLiteController sqlController = new SQLiteController();
                List<ClientOutput> clientOutputList = sqlController.LastValuesFromDB();
                if (clientOutputList != null || clientOutputList.Count != 0)
                {
                    List<ClientOutput> criticalValues = GetCriticalValues(clientOutputList);
                    if (criticalValues.Count > 0)
                    {
                        GetContext().Clients.Group("Clients").PreviewCritical(criticalValues);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private static List<ClientOutput> GetCriticalValues( List<ClientOutput> clientOutputList )
        {
            List<ClientOutput> newClientOutputList = new List<ClientOutput>();
            foreach (ClientOutput co in clientOutputList)
            {
                ClientOutput newClientOutput = new ClientOutput(co.PCName, co.ID, co.Group);
                foreach (PluginOutputCollection pluginCollection in co.CollectionList)
                {
                    PluginOutputCollection newPluginCollection = new PluginOutputCollection();
                    newPluginCollection.PluginName = pluginCollection.PluginName;
                    newPluginCollection.PluginUID = pluginCollection.PluginUID;
                    foreach (PluginOutput pluginOutput in pluginCollection.PluginOutputList)
                    {
                        if (pluginOutput.Values.Any(item => item.IsCritical == true))
                        {
                            newPluginCollection.PluginOutputList.Add(pluginOutput);
                        }
                    }
                    if (newPluginCollection.PluginOutputList.Count > 0)
                    {
                        newClientOutput.CollectionList.Add(newPluginCollection);
                    }
                }
                if (newClientOutput.CollectionList.Count > 0)
                {
                    newClientOutputList.Add(newClientOutput);
                }
            }
            return newClientOutputList;
        }

        private static void OneMachineView()
        {
            try
            {
                SQLiteController sqlController = new SQLiteController();
                ClientOutput clientOutput = sqlController.JSONFromSQL(_group, _nodeID, _pcName);
                if (clientOutput != null)
                {
                    GetContext().Clients.Group("Clients").PluginsMessage(clientOutput);
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
            _changed = true;
        }

        public static void SetPCName(string pcName)
        {
            _pcName = pcName;
            _changed = true;
        }

        public static void SetGroup(string group)
        {
            _group = group;
            _changed = true;
        }

        public static void SetView(ViewType viewType)
        {
            _viewType = viewType;
            _changed = true;
        }

        public static void LoadTreeView()
        {
            SQLiteController sqlConroller = new SQLiteController();
            List<ClientOutput> treeInfo = sqlConroller.GetBasicInfo();
            foreach (ClientOutput node in treeInfo)
            {
                GetContext().Clients.Group("Clients").ActivateTree(node);
            }
        }

        public static void InitDatabase()
        {
            SQLiteController sqlController = new SQLiteController();

            sqlController.CreateDbFile();
            sqlController.CreateTables();
            sqlController.InitPlugins();
        }

        public static void JSONToSQL(ClientOutput clientOutput)
        {
            SQLiteController sqlController = new SQLiteController();

            sqlController.JSONToSQL(clientOutput);
        }

        public static void SaveBasicInfoToDB(ClientOutput clientOutput)
        {
            SQLiteController sqlController = new SQLiteController();

            sqlController.SaveBasicInfo(clientOutput);
        }

        public static void SavePosition(string computerID, string pluginGuid, int posTop, int posLeft)
        {
            SQLiteController sqlController = new SQLiteController();

            sqlController.SaveHTMLPosition(computerID, pluginGuid, posTop, posLeft);
        }

        public static void SendSavedPosition()
        {
            SQLiteController sqlController = new SQLiteController();

            //var machineID = sqlConroller.GetMachineID(_nodeID);
            List<PluginSettings> pluginPositions = sqlController.GetHTMLPositions(_nodeID);
            if (pluginPositions != null && pluginPositions.Count > 0)
            {
                try
                {
                    GetContext().Clients.Group("Clients").SavePositionToLocalStorage(pluginPositions.ToArray());
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }

        public static void SendPluginSettings()
        {
            SQLiteController sqlController = new SQLiteController();

            List<PluginSettings> pluginSettingsList = sqlController.GetAllPluginSettings();
            if (pluginSettingsList != null && pluginSettingsList.Count > 0)
            {
                try
                {
                    GetContext().Clients.Group("Clients").SaveSettingsToLocalStorage(pluginSettingsList.ToArray());
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }
    }
}