using System;
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

            memoryUsage = (Math.Round((GetTotalMemoryInBytes() / (1024 * 1024 * 1024)), 2)).ToString();
            memoryUsage += " GB";
            _pluginOutputs.PluginOutputList.Add(new PluginOutput("Free RAM", memoryUsage, false));
            return _pluginOutputs;
        }

        static double GetTotalMemoryInBytes()
        {
            return new ComputerInfo().AvailablePhysicalMemory;
        }
    }
}
