using MonitoringAgent;
using System.Management;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.NetworkInformation;

namespace MachineIdentifier
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

            foreach (ManagementObject mo in moc)
            {
                cpuInfo = mo.Properties["processorID"].Value.ToString();
                _pluginOutputs.NewPluginOutput("CPU ID", cpuInfo);
            }
            /*var macAddr =
            (
                from nic in NetworkInterface.GetAllNetworkInterfaces()
                where nic.OperationalStatus == OperationalStatus.Up
                select nic.GetPhysicalAddress().ToString()
            ).FirstOrDefault();
            _pluginOutputs.NewPluginOutput("MAC ID", macAddr);*/
            return _pluginOutputs;
        }
    }
}
