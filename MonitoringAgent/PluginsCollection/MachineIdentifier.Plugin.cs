using System.Management;

namespace PluginsCollection
{
    public class MachineIdentifier : IPlugin
    {
        PluginOutputCollection _pluginOutputs;

        public string Name
        {
            get
            {
                return "Machine identifier";
            }
        }
        public MachineIdentifier()
        {
            _pluginOutputs = new PluginOutputCollection(Name);
        }

        public PluginOutputCollection Output()
        {
            string cpuInfo = string.Empty;
            ManagementClass mc = new ManagementClass("win32_processor");
            ManagementObjectCollection moc = mc.GetInstances();

            _pluginOutputs.PluginOutputList.Clear();
            foreach (ManagementObject mo in moc)
            {
                cpuInfo = mo.Properties["processorID"].Value.ToString();
                _pluginOutputs.NewPluginOutput("CPU ID", cpuInfo, false);
            }
            return _pluginOutputs;
        }
    }
}
