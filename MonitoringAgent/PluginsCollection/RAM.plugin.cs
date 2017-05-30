using System;
using Microsoft.VisualBasic.Devices;
using System.Collections.Generic;

namespace PluginsCollection
{
    public class RAM : IPlugin
    {
        PluginOutputCollection _pluginOutputs;

        public Guid PluginUID
        {
            get
            {
                return new Guid("eaabddc4-d976-4e26-bed9-33772da99bfb");
            }
        }

        public string PluginName
        {
            get
            {
                return "RAM usage";
            }
        }

        public PluginType PluginType
        {
            get
            {
                return PluginType.Table;
            }
        }

        public RAM()
        {
            _pluginOutputs = new PluginOutputCollection();
            _pluginOutputs.PluginUID = PluginUID;
            _pluginOutputs.PluginName = PluginName;
        }

        public PluginOutputCollection Output()
        {
            string memoryUsage = string.Empty;
            List<SimplePluginOutput> listSPO = new List<SimplePluginOutput>();
            _pluginOutputs.PluginOutputList.Clear();

            memoryUsage = (Math.Round((GetFreeMemoryInBytes() / (1024 * 1024 * 1024)), 2)).ToString();
            memoryUsage += " GB";

            if (((GetFreeMemoryInBytes() / GetTotalMemoryInBytes()) * 100) < 10)
            {
                listSPO.Add(new SimplePluginOutput(memoryUsage, true));
            }
            else
            {
                listSPO.Add(new SimplePluginOutput(memoryUsage, false));
            }
            _pluginOutputs.PluginOutputList.Add(new PluginOutput("Free RAM", listSPO));
            return _pluginOutputs;
        }

        double GetFreeMemoryInBytes()
        {
            return new ComputerInfo().AvailablePhysicalMemory;
        }

        double GetTotalMemoryInBytes()
        {
            return new ComputerInfo().TotalPhysicalMemory;
        }
    }
}
