using System;
using Microsoft.VisualBasic.Devices;
using System.Collections.Generic;

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
            List<SimplePluginOutput> listSPO = new List<SimplePluginOutput>();
            _pluginOutputs.PluginOutputList.Clear();

            memoryUsage = (Math.Round((GetTotalMemoryInBytes() / (1024 * 1024 * 1024)), 2)).ToString();
            memoryUsage += " GB";

            listSPO.Add(new SimplePluginOutput(memoryUsage, true));
            _pluginOutputs.PluginOutputList.Add(new PluginOutput("Free RAM", listSPO));
            return _pluginOutputs;
        }

        static double GetTotalMemoryInBytes()
        {
            return new ComputerInfo().AvailablePhysicalMemory;
        }
    }
}
