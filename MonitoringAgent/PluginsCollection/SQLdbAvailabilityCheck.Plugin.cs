﻿using System;
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
                return "SQLAvailability";
            }
        }

        public string connString = TPOMM.Tools.Config.Current.cfConnectionString.ToString();

        public SQLdbAvailabilityCheck()
        {
            _pluginOutputs = new PluginOutputCollection(Name);
        }

        public static bool IsServerConnected(string connectionString)
        {
            using (SqlConnection connection = new SqlConnection(TPOMM.Tools.Config.Current.ConnectionStrings["CFConnectionString"]))
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
            List<SimplePluginOutput> listSPO = new List<SimplePluginOutput>();
            _pluginOutputs.PluginOutputList.Clear();

            if (IsServerConnected(connString))
            {
                listSPO.Add(new SimplePluginOutput("Connnected", false));
            }
            else
            {
                listSPO.Add(new SimplePluginOutput("Disconnnected", true));
            }
            _pluginOutputs.PluginOutputList.Add(new PluginOutput("SQL Status", listSPO));

            return _pluginOutputs;
        }
    }
}
