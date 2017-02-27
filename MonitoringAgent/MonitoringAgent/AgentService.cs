using System;
using System.Net;
using System.Threading;
using Newtonsoft.Json;
using System.Collections.Generic;
using PluginsCollection;
using System.Configuration;
using System.Net.NetworkInformation;
using System.Linq;
using log4net;
using Microsoft.AspNet.SignalR.Client;

namespace MonitoringAgent
{
    public class AgentService 
    {
        // Logging initialization
        private static readonly ILog _log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        static PluginLoader _plugins;
        static bool _running = true;
        
        public string _dbName = ConfigurationManager.AppSettings["DatabasePath"];

        public void Start()
        {
            // Application Running Information
            _log.Info("Application is RUNNING");

            // Load Plugins
            _plugins = new PluginLoader();
            _plugins.Loader();

            StartThread();
            // write code here that runs when the Windows Service starts up.  
        }

        public void Stop()
        {
            // write code here that runs when the Windows Service stops.  
        }

        private void StartThread()
        {
            Thread pollingThread = null;

            try
            {
                // Create a new thread to start polling and sending the data
                pollingThread = new Thread(new ThreadStart(RunPollingThread));
                pollingThread.Start();

                Console.WriteLine("Starting thread..");
                _log.Info("Starting thread...");

                pollingThread.Join(5000);

            }
            catch (Exception)
            {
                pollingThread.Abort();
                throw;
            }

            // Wait for key
            Console.ReadLine();
        }

        private void RunPollingThread()
        {
            // Convert the object that was passed in
            DateTime lastPollTime = DateTime.MinValue;

            Console.WriteLine("Started polling...");
            _log.Info("Started polling...");

            CreateDataBaseAndTable();
            string serverIP = ConfigurationManager.AppSettings["ServerIP"];
            HubConnection hubConnection = new HubConnection(serverIP);
            IHubProxy monitoringHub = hubConnection.CreateHubProxy("MyHub");
            //hubConnection.Received += data => Console.WriteLine(data);

            hubConnection.Start().ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    Console.WriteLine("There was an error opening the connection: {0}",
                              task.Exception.GetBaseException());
                }
                else
                {
                    Console.WriteLine("Connected");
                }
            }).Wait();

            TakeAndPostData(hubConnection, monitoringHub, true);
            Thread.Sleep(1000);
            TakeAndPostData(hubConnection, monitoringHub, true);

            // Start the polling loop
            while (_running)
            {
                // Poll every 5 second
                if ((DateTime.Now - lastPollTime).TotalMilliseconds >= 5000)
                {
                    TakeAndPostData(hubConnection, monitoringHub);
                    
                    // Reset the poll time
                    lastPollTime = DateTime.Now;
                }
                else
                {
                    Thread.Sleep(10);
                }
            }
            hubConnection.Stop();
        }

        private void TakeAndPostData(HubConnection hubConnection, IHubProxy monitoringHub, bool initPost = false)
        {
            string json;
            string serverIP = ConfigurationManager.AppSettings["ServerIP"];
            string customerName = ConfigurationManager.AppSettings["CustomerName"];
            ClientOutput output = new ClientOutput(getPCName(), getMACAddress(), customerName);
            WebClient client = new WebClient();

            PluginOutputCollection plugOutput = new PluginOutputCollection();

            json = string.Empty;
            output.CollectionList.Clear();

            if (!initPost)
            {
                foreach (var plugin in _plugins.pluginList)
                {
                    plugOutput = plugin.Output();
                    if (plugOutput != null)
                    {
                        output.CollectionList.Add(plugOutput);
                    }
                }
            }
            else
            {
                output.InitPost = true;
            }

            json = JsonConvert.SerializeObject(output);
            client.Headers.Add("Content-Type", "application/json");

            bool connectionStatus = true;
            //bool connectionStatus = false;
            //connectionStatus = CheckConnection(serverIP, monitoringHub);

            if (connectionStatus)
            {
                try
                {                
                    /*monitoringHub.Invoke<string>("Send", "test").ContinueWith(task => {
                        if (task.IsFaulted)
                        {
                            Console.WriteLine("There was an error calling send: {0}",
                                              task.Exception.GetBaseException());
                        }
                        else
                        {
                            Console.WriteLine(task.Result);
                        }
                    });*/

                    /*monitoringHub.On<string>("addMessage", param => {
                        Console.WriteLine(param);
                    });*/

                    //monitoringHub.Invoke<string>("DoSome", "I'm doing something!!!").Wait();

                    monitoringHub.Invoke<ClientOutput>("SendPluginOutput", output).Wait();

                    //client.UploadString(serverIP, json);
                }
                catch (Exception err)
                {
                    _log.Error("Upload string ERROR: ", err);
                    hubConnection.Stop();
                    SaveOutputToDB(json);
                    return;
                }
                try
                {
                    Dictionary<int, string> dbValues = new Dictionary<int, string>();
                    dbValues = SQLiteDB.GetStoredJson(_dbName);
                    foreach (var item in dbValues)
                    {
                        string jsonDB = item.Value;
                        client.UploadString(serverIP, jsonDB);
                        SQLiteDB.UpdateStatus(_dbName, item.Key);
                    }
                }
                catch (Exception err)
                {
                    _log.Error("Read stored values from database", err);
                    return;
                }
            }
            else
            {
                SaveOutputToDB(json);
            }
        }

        private void CreateDataBaseAndTable()
        {
            SQLiteDB.CreateDbFile(_dbName);
            SQLiteDB.CreateTable(_dbName);
        }

        private void SaveOutputToDB(string json)
        {
            SQLiteDB.InsertToDb(_dbName, json);

            _log.Warn("Server unreachable, writing into local db");
        }

        public string getMACAddress()
        {
            NetworkInterface[] NI = NetworkInterface.GetAllNetworkInterfaces();
            NetworkInterface ni = NI.FirstOrDefault(x => x.OperationalStatus == OperationalStatus.Up);
            return ni.GetPhysicalAddress().ToString();
        }

        public string getPCName()
        {
            string pcName = ConfigurationManager.AppSettings["PCName"];

            if (pcName != null && pcName != string.Empty)
            {
                return pcName;
            }
            else
            {
                return Environment.MachineName;
            }
        }

        private bool CheckConnection( String URL, IHubProxy monitoringHub )
        {
            bool result = false;
            try
            {
                monitoringHub.On<string>("addMessage", param => {
                    result = true;
                });

                monitoringHub.Invoke<string>("DoSome", "I'm doing something!!!").Wait();

                monitoringHub.Invoke("CheckConnection", "test").ContinueWith((task) => 
                {
                    result = task.IsCompleted;
                });
            }
            catch (Exception ex)
            {
                _log.Error("Server connection fail.", ex);
                throw ex;
            }
            return result;
        }
    }
}