using System;
using System.Security.Principal;
using Cassia;
using System.Collections.Generic;

namespace PluginsCollection
{
    public class TerminalServicesUsers : IPlugin
    {
        PluginOutputCollection _pluginOutputs;

        public string Name
        {
            get
            {
                return "Logged-in TS users";
            }
        }

        public TerminalServicesUsers()
        {
            _pluginOutputs = new PluginOutputCollection(Name);
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
