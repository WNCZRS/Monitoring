using System;
using System.Net;
using System.Threading;
using Newtonsoft.Json;
using System.Collections.Generic;
using PluginsCollection;
using System.Configuration;
using System.Text;

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

              //  _running = false;

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
            List<PluginOutputCollection> outputList;
            WebClient client;
            string json;
            string serverIP = ConfigurationManager.AppSettings["ServerIP"];

            // Convert the object that was passed in
            DateTime lastPollTime = DateTime.MinValue;

            Console.WriteLine("Started polling...");
            _log.Info("Started polling...");

            // Start the polling loop
            while (_running)
            {
                // Poll every second
                if ((DateTime.Now - lastPollTime).TotalMilliseconds >= 1000)
                {
                    client = new WebClient();
                    json = string.Empty;
                    outputList = new List<PluginOutputCollection>();

                    foreach (var plugin in _plugins.pluginList)
                    {
                        outputList.Add(plugin.Output());
                    }

                    json = JsonConvert.SerializeObject(outputList);
                    client.Headers.Add("Content-Type", "application/json");
                    client.UploadString(serverIP, json);

                    // Reset the poll time
                    lastPollTime = DateTime.Now;

                }
                else
                {
                    Thread.Sleep(10);
                }
            }
        }
    }
}