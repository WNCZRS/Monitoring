using System.Linq;
using System.Net.NetworkInformation;

namespace PluginsCollection
{
    public class MACAddress : IPlugin
    {
        PluginOutputCollection _pluginOutputs;
        public string Name
        {
            get
            {
                return "MAC Address";
            }
        }

        public MACAddress()
        {
            _pluginOutputs = new PluginOutputCollection(Name);
        }

        public PluginOutputCollection Output()
        {
            _pluginOutputs.PluginOutputList.Clear();
            var macAddr =
            (
                from nic in NetworkInterface.GetAllNetworkInterfaces()
                where nic.OperationalStatus == OperationalStatus.Up
                select nic.GetPhysicalAddress().ToString()
            ).FirstOrDefault();
            _pluginOutputs.NewPluginOutput("MAC ID", macAddr);

            return _pluginOutputs;
        }
    }
}
