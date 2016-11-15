using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Principal;
using Cassia;

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
            using (ITerminalServer server = manager.GetRemoteServer(Environment.MachineName.ToString()))
            {
                server.Open();
                foreach (ITerminalServicesSession session in server.GetSessions())
                {
                    NTAccount account = session.UserAccount;
                    if (account != null)
                    {
                        _pluginOutputs.NewPluginOutput("User", account.ToString());
                    }
                }
            }
            return _pluginOutputs;
        }
    }
}
