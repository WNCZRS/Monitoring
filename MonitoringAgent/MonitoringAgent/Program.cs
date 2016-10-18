using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;

namespace MonitoringAgent
{
    static class Program
    {
        public static void Main(string[] args)
        {
            var instance = new PluginLoader();
            instance.Loader();

            Console.WriteLine("Application is RUNNING");
            Console.ReadLine();
        }

    }
}
