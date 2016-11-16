using System;
using System.Net;
using System.Threading;
using Newtonsoft.Json;
using System.Collections.Generic;
using PluginsCollection;
using System.Configuration;

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
            // Convert the object that was passed in
            DateTime lastPollTime = DateTime.MinValue;

            Console.WriteLine("Started polling...");
            _log.Info("Started polling...");

            // Start the polling loop
            while (_running)
            {
                outputList = new List<PluginOutputCollection>();

                // Poll every second
                if ((DateTime.Now - lastPollTime).TotalMilliseconds >= 1000)
                {
                    WebClient client = new WebClient();
                    string json = string.Empty;
                    foreach (var plugin in _plugins.pluginList)
                    {
                        //outputList.Add(plugin.Output());
                        PluginOutputCollection poc = plugin.Output();
                        string serverIP = ConfigurationManager.AppSettings["ServerIP"];

                        foreach (PluginOutput po in poc.PluginOuputList)
                        {
                            json = JsonConvert.SerializeObject(new { poc.PluginName, po.PropertyName, po.Value });
                            client.Headers.Add("Content-Type", "application/json");
                            client.UploadString(serverIP, json);
                        }
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

        /*static void GetMetrics(out double processorTime, out ulong memUsage, out ulong totalMemory)
        {
            processorTime = (double)_cpuCounter.NextValue();
            memUsage = (ulong)_memUsageCounter.NextValue();
            totalMemory = 0;

            // Get total memory from WMI
            ObjectQuery memQuery = new ObjectQuery("SELECT * FROM CIM_OperatingSystem");

            ManagementObjectSearcher searcher = new ManagementObjectSearcher(memQuery);

            foreach (ManagementObject item in searcher.Get())
            {
                totalMemory = (ulong)item["TotalVisibleMemorySize"];
            }
        } */
    }
}