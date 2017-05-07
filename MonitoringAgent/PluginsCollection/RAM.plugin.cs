using System;
using Microsoft.VisualBasic.Devices;
using System.Collections.Generic;

namespace PluginsCollection
{
    public class RAM : IPlugin
    {
        PluginOutputCollection _pluginOutputs;

        public Guid UID
        {
            get
            {
                return new Guid("eaabddc4-d976-4e26-bed9-33772da99bfb");
            }
        }

        public string Name
        {
            get
            {
                return "RAM usage";
            }
        }

        public PluginType Type
        {
            get
            {
                return PluginType.Table;
            }
        }

        public RAM()
        {
            _pluginOutputs = new PluginOutputCollection();
            _pluginOutputs.PluginUID = UID;
            _pluginOutputs.PluginName = Name;
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
