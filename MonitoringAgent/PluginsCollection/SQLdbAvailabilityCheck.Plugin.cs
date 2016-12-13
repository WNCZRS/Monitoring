using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TPOMM.Tools;

namespace PluginsCollection
{
    public class SQLdbAvailabilityCheck : IPlugin
    {
        PluginOutputCollection _pluginOutputs;
        public string Name
        {
            get
            {
                return "SQL";
            }
        }

        public string connString = TPOMM.Tools.Config.Current.cfConnectionString.ToString();

        public SQLdbAvailabilityCheck ()
        {
            _pluginOutputs = new PluginOutputCollection(Name);
        }

        public static bool IsServerConnected(string connectionString)
        {
            using (SqlConnection connection = new SqlConnection(TPOMM.Tools.Config.Current.cfConnectionString.ToString()))
            {
                try
                {
                    connection.Open();
                    return true;
                }
                catch (SqlException)
                {
                    return false;
                }
            }
        }

        public PluginOutputCollection Output()
        {
            if (IsServerConnected(connString))
            {
                _pluginOutputs.NewPluginOutput("SQL Status", "Connected", true);
            }
            else
            {
                _pluginOutputs.NewPluginOutput("SQL Status", "Connected", false);
            }
            return _pluginOutputs;
        }
    }
}
