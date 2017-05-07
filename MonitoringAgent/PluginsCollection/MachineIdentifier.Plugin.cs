using System;
using System.Collections.Generic;
using System.Management;

namespace PluginsCollection
{
    public class MachineIdentifier : IPlugin
    {
        PluginOutputCollection _pluginOutputs;

        public Guid UID
        {
            get
            {
                return new Guid("c03a6eb7-7b2d-48c7-a015-52f38821bdbe");
            }
        }

        public string Name
        {
            get
            {
                return "Machine identifier";
            }
        }

        public PluginType Type
        {
            get
            {
                return PluginType.Table;
            }
        }

        public MachineIdentifier()
        {
            _pluginOutputs = new PluginOutputCollection();
            _pluginOutputs.PluginUID = UID;
            _pluginOutputs.PluginName = Name;
        }

        public PluginOutputCollection Output()
        {
            string cpuInfo = string.Empty;
            ManagementClass mc = new ManagementClass("win32_processor");
            ManagementObjectCollection moc = mc.GetInstances();
            List<SimplePluginOutput> listSPO = new List<SimplePluginOutput>();

            _pluginOutputs.PluginOutputList.Clear();
            foreach (ManagementObject mo in moc)
            {
                cpuInfo = mo.Properties["processorID"].Value.ToString();
                listSPO.Add(new SimplePluginOutput(cpuInfo, false));
                _pluginOutputs.PluginOutputList.Add(new PluginOutput("CPU ID", listSPO));
            }
            return _pluginOutputs;
        }
    }
}
