﻿using Microsoft.AspNet.SignalR;
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
    public enum ViewType
    {
        OneMachine,
        CriticalPreview
    }
    public class MessageController
    {
        private static string _nodeID;
        private static string _customer;
        private static string _pcName;
        private static bool _changed;
        private static ViewType _viewType;

        public ViewType ViewType
        {
            get
            {
                return _viewType;
            }
            set
            {
                _viewType = value;
            }
        }

        private MessageController()
        {
            _customer = string.Empty;
            _nodeID = string.Empty;
            _pcName = string.Empty;
            _changed = false;
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
            GetContext().Clients.All.InitMainDiv();
            try
            {
                List<ClientOutput> clientOutputList = SQLiteController.LastValuesFromDB();
                if (clientOutputList != null || clientOutputList.Count != 0)
                {
                    List<ClientOutput> criticalValues = GetCriticalValues(clientOutputList);
                    if (criticalValues.Count > 0)
                    {
                        GetContext().Clients.All.previewCritical(criticalValues);
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
                ClientOutput newClientOutput = new ClientOutput(co.PCName, co.ID, co.Customer);
                foreach (PluginOutputCollection pluginCollection in co.CollectionList)
                {
                    PluginOutputCollection newPluginCollection = new PluginOutputCollection(pluginCollection.PluginName);
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
            _changed = true;
        }

        public static void SetPCName(string pcName)
        {
            _pcName = pcName;
            _changed = true;
        }

        public static void SetCustomer(string customer)
        {
            _customer = customer;
            _changed = true;
        }

        public static void SetView(ViewType viewType)
        {
            _viewType = viewType;
        }

        public static void SwitchView()
        {
            if (_viewType == ViewType.CriticalPreview)
            {
                GetContext().Clients.All.InitMainDiv();
                _viewType = ViewType.OneMachine;
                LoadTreeView();
            }
            else
            {
                _viewType = ViewType.CriticalPreview;
                GetContext().Clients.All.deactivateTree();
            }
            _changed = true;
        }

        public static void LoadTreeView()
        {
            if (_viewType == ViewType.OneMachine)
            {
                List<ClientOutput> treeInfo = SQLiteController.GetBasicInfo();
                foreach (ClientOutput node in treeInfo)
                {
                    GetContext().Clients.All.activateTree(node);
                }
            }
        }
    }
}