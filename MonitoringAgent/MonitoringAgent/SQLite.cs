using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;

namespace MonitoringAgent
{
    class SQLite
    {
        public void CreateDbFile (string fileName)
        {
            SQLiteConnection.CreateFile(fileName);
        }

        public void ConnectToDB ()
        {
            SQLiteConnection m_dbConnection;
            m_dbConnection = new SQLiteConnection("Data Source=MyDatabase.sqlite;Version=3;");
            m_dbConnection.Open();
        }

        public void CreateTable (string tableName, Array fields)
        {
            //string sql = ("create table {0} (name varchar(20), score int)", tableName);
        }
        
    }
}
