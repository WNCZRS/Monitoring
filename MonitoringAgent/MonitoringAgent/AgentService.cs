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
            _plugins.Load();

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

            //hubConnection.Received += data => Console.WriteLine(data);

            TakeAndPostData(true);
            Thread.Sleep(1000);
            TakeAndPostData(true);

            // Start the polling loop
            while (_running)
            {
                // Poll every 5 second
                if ((DateTime.Now - lastPollTime).TotalMilliseconds >= 5000)
                {
                    TakeAndPostData();
                    
                    // Reset the poll time
                    lastPollTime = DateTime.Now;
                }
                else
                {
                    Thread.Sleep(10);
                }
            } 
        }

        private void TakeAndPostData(bool initPost = false)
        {
            string json;
            string serverIP = ConfigurationManager.AppSettings["ServerIP"];
            string customerName = ConfigurationManager.AppSettings["CustomerName"];
            ClientOutput output = new ClientOutput(getPCName(), getMACAddress(), customerName);

            PluginOutputCollection plugOutput = new PluginOutputCollection();

            json = string.Empty;
            output.CollectionList.Clear();

            if (!initPost)
            {
                foreach (var plugin in _plugins.PluginList)
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

            if (!SendPluginOutput(output))
            {
                SaveOutputToDB(JsonConvert.SerializeObject(output));
                return;
            }

            try
            {
                Dictionary<int, string> dbValues = new Dictionary<int, string>();
                dbValues = SQLiteDB.GetStoredJson(_dbName);
                foreach (var item in dbValues)
                {
                    ClientOutput clientOutputDB = JsonConvert.DeserializeObject<ClientOutput>(item.Value);
                    if (SendPluginOutput(output))
                    {
                        SQLiteDB.UpdateStatus(_dbName, item.Key);
                    }
                }
            }
            catch (Exception err)
            {
                _log.Error("Read stored values from database", err);
                return;
            }
            
        }

        private bool SendPluginOutput(ClientOutput output)
        {
            bool sended = true;
            string serverIP = ConfigurationManager.AppSettings["ServerIP"];
            HubConnection hubConnection = new HubConnection(serverIP);
            IHubProxy monitoringHub = hubConnection.CreateHubProxy("MyHub");

            try
            {
                hubConnection.Start().ContinueWith(task =>
                {
                    if (task.IsFaulted)
                    {
                        Console.WriteLine($"There was an error opening the connection: {task.Exception.GetBaseException()}");
                        sended = false;
                    }
                    else
                    {
                        Console.WriteLine("Connected");
                    }
                }).Wait();

                monitoringHub.Invoke<ClientOutput>("SendPluginOutput", output).Wait();
            }
            catch (Exception ex)
            {
                _log.Error("Upload string Exception: ", ex);
                sended = false;
                throw ex;                
            }
            finally
            {
                hubConnection.Stop();
            }
            return sended;
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
    }
}