using System;
using System.Diagnostics;
using System.Net;
using System.Threading;
using System.Configuration;
using System.Management;
using Newtonsoft.Json;
using System.Text;
using System.Collections.Generic;

namespace MonitoringAgent
{
    class Program
    {
        static PluginLoader _plugins;
        static bool _running = true;
        static PerformanceCounter _cpuCounter, _memUsageCounter;

        public static void Main(string[] args)
        {
            // Application Running Information
            Console.WriteLine("Application is RUNNING");

            // Load Plugins
            _plugins = new PluginLoader();
            _plugins.Loader();

            StartThread(); 
        }

        private static void StartThread()
        {
            Thread pollingThread = null;

            try
            {
                _cpuCounter = new PerformanceCounter();
                _cpuCounter.CategoryName = "Processor";
                _cpuCounter.CounterName = "% Processor Time";
                _cpuCounter.InstanceName = "_Total";

                _memUsageCounter = new PerformanceCounter("Memory", "Available KBytes");

                // Create a new thread to start polling and sending the data
                pollingThread = new Thread(new ParameterizedThreadStart(RunPollingThread));
                pollingThread.Start();

                Console.WriteLine("Press a key to stop and exit");
                Console.ReadKey();

                Console.WriteLine("Stopping thread..");

                _running = false;

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
            List<Dictionary<string, string>> outputList;
            // Convert the object that was passed in
            DateTime lastPollTime = DateTime.MinValue;

            Console.WriteLine("Started polling...");

            // Start the polling loop
            while (_running)
            {
                outputList = new List<Dictionary<string, string>>();

                // Poll every second
                if ((DateTime.Now - lastPollTime).TotalMilliseconds >= 1000)
                {
                    foreach (var plugin in _plugins.pluginList)
                    {
                        outputList.Add(plugin.Output());
                    }

                    double cpuTime;
                    ulong memUsage, totalMemory;

                    // Get the stuff we need to send
                    GetMetrics(out cpuTime, out memUsage, out totalMemory);

                    // Send the data
                    var postData = new
                    {
                        MachineName = System.Environment.MachineName,
                        Processor = cpuTime,
                        MemUsage = memUsage,
                        TotalMemory = totalMemory
                    };

                    var json = JsonConvert.SerializeObject(postData);

                    // Post the data to the server
                    //var serverUrl = new Uri(ConfigurationManager.AppSettings["ServerUrl"]);

                    var client = new WebClient();
                    client.Headers.Add("Content-Type", "application/json");
                    //client.UploadString(serverUrl, json);
                    //client.UploadString("http://192.168.0.105:15123/api/cpuinfo", json);
                    client.UploadString("http://localhost:8000/api/cpuinfo", json);

                    // Reset the poll time
                    lastPollTime = DateTime.Now;
                }
                else
                {
                    Thread.Sleep(10);
                }
            }
        }

        static void GetMetrics(out double processorTime, out ulong memUsage, out ulong totalMemory)
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
        }

    }
}
