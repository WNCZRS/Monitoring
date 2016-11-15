using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PluginsCollection
{
    public class SQLdbAvailabilityCheck : IPlugin
    {
        /*public void Output()
        {
            SqlConnection conn = new SqlConnection();
            conn.ConnectionString = ConfigurationManager.ConnectionStrings["TPOMM"].ConnectionString;
            Console.WriteLine(IsServerConnected(conn.ConnectionString));
        }
        public static bool IsServerConnected(string connectionString)
        {
            using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["SQLConnection"].ToString()))
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
        }*/
        public string Name
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public PluginOutputCollection Output()
        {
            throw new NotImplementedException();
        }
    }
}
