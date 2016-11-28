using System;
using System.Net;
using System.Threading;
using Newtonsoft.Json;
using System.Collections.Generic;
using PluginsCollection;
using System.Configuration;
using System.Text;
using System.Net.NetworkInformation;
using System.Linq;

namespace MonitoringAgent
{
    public class AgentService
    {
        // Logging initialization
        private static readonly log4net.ILog _log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        static PluginLoader _plugins;
        static bool _running = true;
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

        private static void StartThread()
        {
            Thread pollingThread = null;

            try
            {
                // Create a new thread to start polling and sending the data
                pollingThread = new Thread(new ParameterizedThreadStart(RunPollingThread));
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

        static void RunPollingThread(object data)
        {
            string json;
            string serverIP = ConfigurationManager.AppSettings["ServerIP"];
            WebClient client = new WebClient();
            ClientOutput output = new ClientOutput(getPCName(), getMACAddress());
            PluginOutputCollection plugOutput = new PluginOutputCollection();

            // Convert the object that was passed in
            DateTime lastPollTime = DateTime.MinValue;

            Console.WriteLine("Started polling...");
            _log.Info("Started polling...");

            // Start the polling loop
            while (_running)
            {
                // Poll every 5 second
                if ((DateTime.Now - lastPollTime).TotalMilliseconds >= 5000)
                {
                    json = string.Empty;
                    output.CollectionList.Clear();

                    foreach (var plugin in _plugins.pluginList)
                    {
                        plugOutput = plugin.Output();
                        if (plugOutput != null)
                        {
                            output.CollectionList.Add(plugOutput);
                        }
                    }

                    json = JsonConvert.SerializeObject(output);
                    client.Headers.Add("Content-Type", "application/json");

                    bool connectionStatus = false;
                    connectionStatus = CheckConnection(serverIP);

                    if (connectionStatus)
                    {
                        try
                        {
                            client.UploadString(serverIP, json);
                        }
                        catch (Exception err)
                        {
                            _log.Error("Upload string ERROR: ", err);
                            continue;
                        }
                        try
                        {
                            Dictionary<int, string> dbValues = new Dictionary<int, string>();
                            string dbName = "MonitoringAgentDB.sqlite";
                            dbValues = SQLiteDB.GetStoredJson(dbName);
                            foreach (var item in dbValues)
                            {
                                string jsonDB = item.Value;
                                client.UploadString(serverIP, jsonDB);
                                SQLiteDB.UpdateStatus(dbName, item.Key);
                            }
                        }
                        catch (Exception err)
                        {
                            _log.Error("Read stored values from database", err);
                            continue;
                        }
                    }
                    else
                    {
                        string dbName = "MonitoringAgentDB.sqlite";
                        SQLiteDB.CreateDbFile(dbName);
                        SQLiteDB.CreateTable(dbName);
                        SQLiteDB.InsertToDb(dbName, json);

                        _log.Warn("Server unreachable, writing into local db");
                    }

                    
                    // Reset the poll time
                    lastPollTime = DateTime.Now;
                }
                else
                {
                    Thread.Sleep(10);
                }
            }
        }

        public static string getMACAddress()
        {
            NetworkInterface[] NI = NetworkInterface.GetAllNetworkInterfaces();
            NetworkInterface ni = NI.FirstOrDefault(x => x.OperationalStatus == OperationalStatus.Up);
            return ni.GetPhysicalAddress().ToString();
        }

        public static string getPCName()
        {
            return Environment.MachineName;
        }

        private static bool CheckConnection(String URL)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(URL);
                request.Timeout = 5000;
                request.Credentials = CredentialCache.DefaultNetworkCredentials;
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                if (response.StatusCode == HttpStatusCode.OK) return true;
                else return false;
            }
            catch
            {
                return false;
            }
        }
    }
}