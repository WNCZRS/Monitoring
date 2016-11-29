using System;
using System.IO;
using Microsoft.VisualBasic.Devices;

namespace PluginsCollection
{
    public class RAM : IPlugin
    {
        PluginOutputCollection _pluginOutputs;

        public string Name
        {
            get
            {
                return "RAM usage";
            }
        }

        public RAM()
        {
            _pluginOutputs = new PluginOutputCollection(Name);
        }

        public PluginOutputCollection Output()
        {
            string memoryUsage = string.Empty;
            _pluginOutputs.PluginOutputList.Clear();

            memoryUsage = GetTotalMemoryInBytes().ToString();
           
            return _pluginOutputs;
        }
        static ulong GetTotalMemoryInBytes()
        {
            return new Microsoft.VisualBasic.Devices.ComputerInfo().TotalPhysicalMemory;
        }
    }
}
