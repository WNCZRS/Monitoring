using System.Data.SqlClient;
using System.Configuration;
using System;
using System.Data.Sql;

namespace SQLdbAvailabilityCheck
{
    public class SQLdbAvailabilityCheck
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
    }
}
