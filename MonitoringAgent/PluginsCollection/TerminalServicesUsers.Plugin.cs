using System;
using System.Security.Principal;
using Cassia;
using System.Collections.Generic;

namespace PluginsCollection
{
    public class TerminalServicesUsers : IPlugin
    {
        PluginOutputCollection _pluginOutputs;

        public Guid PluginUID
        {
            get
            {
                return new Guid("736c0953-d609-438a-833f-9a7914e43756");
            }
        }

        public string PluginName
        {
            get
            {
                return "Logged-in TS users";
            }
        }

        public PluginType PluginType
        {
            get
            {
                return PluginType.Table;
            }
        }

        public TerminalServicesUsers()
        {
            _pluginOutputs = new PluginOutputCollection();
            _pluginOutputs.PluginUID = PluginUID;
            _pluginOutputs.PluginName = PluginName;
        }

        public PluginOutputCollection Output()
        {
            ITerminalServicesManager manager = new TerminalServicesManager();
            List<SimplePluginOutput> listSPO = new List<SimplePluginOutput>();

            _pluginOutputs.PluginOutputList.Clear();
            using (ITerminalServer server = manager.GetRemoteServer(Environment.MachineName.ToString()))
            {
                server.Open();
                foreach (ITerminalServicesSession session in server.GetSessions())
                {
                    NTAccount account = session.UserAccount;
                    if (account != null)
                    {
                        listSPO.Add(new SimplePluginOutput(account.ToString(), false));

                        _pluginOutputs.PluginOutputList.Add(new PluginOutput("User", listSPO));
                    }
                }
            }
            return _pluginOutputs;
        }
    }
}
