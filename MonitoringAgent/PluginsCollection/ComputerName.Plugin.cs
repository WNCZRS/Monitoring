using System;

namespace PluginsCollection
{
    class ComputerName : IPlugin
    {
        PluginOutputCollection _pluginOutputs;

        public string Name
        {
            get
            {
                return "Computer name";
            }
        }

        public ComputerName()
        {
            _pluginOutputs = new PluginOutputCollection(Name);
        }
        public PluginOutputCollection Output()
        {
            // Get computer name
            string computerName = string.Empty;
            _pluginOutputs.PluginOuputList.Clear();

            computerName = Environment.MachineName.ToString();

            _pluginOutputs.NewPluginOutput(Name, computerName);

            return _pluginOutputs;
        }
    }
}
