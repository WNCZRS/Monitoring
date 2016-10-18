using System;

namespace MonitoringAgent
{
    static class Program
    {
        public static void Main(string[] args)
        {
            // Application Running Information
            Console.WriteLine("Application is RUNNING");

            // Load Plugins
            var instance = new PluginLoader();
            instance.Loader();

            // Wait for key
            Console.ReadLine();
        }

    }
}
