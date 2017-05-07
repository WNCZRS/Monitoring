using System;
using System.IO;
using Microsoft.Data.Sqlite;
using System.Data;
using System.Collections.Generic;

namespace MonitoringAgent
{
    public class SqliteDB
    {
        public static void CreateDbFile(string fileName)
        {
            if (!File.Exists(fileName))
            {
                SqliteDB.CreateDbFile(fileName);
            }
        }

        public static void CreateTable(string fileName)
        {
            string connectionString = string.Format("Data Source={0};Version=3;", fileName);
            SqliteConnection m_dbConnection;
            m_dbConnection = new SqliteConnection(connectionString);
            m_dbConnection.Open();

            string sql = "CREATE TABLE IF NOT EXISTS [MonitoringAgentCache] ([Id] INTEGER PRIMARY KEY, [Datetime] DATETIME, [Result] TEXT, [Sync] BOOL)";
            SqliteCommand command = new SqliteCommand(sql, m_dbConnection);
            command.ExecuteNonQuery();

            m_dbConnection.Close();
        }

        public static void InsertToDb(string fileName, string result)
        {
            string connectionString = string.Format("Data Source={0};Version=3;", fileName);
            SqliteConnection m_dbConnection;
            m_dbConnection = new SqliteConnection(connectionString);
            m_dbConnection.Open();

            string insertQuery = String.Format("INSERT INTO MonitoringAgentCache (Datetime, Result, Sync) VALUES (DATETIME('NOW'), '{0}', '{1}')", result, false);

            SqliteCommand insertSQL = new SqliteCommand(insertQuery, m_dbConnection);

            try
            {
                insertSQL.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            m_dbConnection.Close();
        }

        public static void CleanDB()
        {

        }

        public static Dictionary<int, string> GetStoredJson(string fileName)
        {
            Dictionary<int, string> dbValues = new Dictionary<int, string>();
            string connectionString = string.Format("Data Source={0};Version=3;", fileName);
            SqliteConnection m_dbConnection;
            m_dbConnection = new SqliteConnection(connectionString);
            m_dbConnection.Open();

            using (SqliteCommand cmd = m_dbConnection.CreateCommand())
            {
                cmd.CommandText = @"SELECT Id, Result FROM MonitoringAgentCache WHERE Sync = 0 ORDER BY Datetime";
                cmd.CommandType = CommandType.Text;
                SqliteDataReader r = cmd.ExecuteReader();
                while (r.Read())
                {
                    dbValues.Add(Convert.ToInt32(r["Id"]), Convert.ToString(r["Result"]));
                }
            }

            m_dbConnection.Close();
            return dbValues;
        }

        public static void UpdateStatus(string fileName, int id)
        {
            string connectionString = string.Format("Data Source={0};Version=3;", fileName);
            SqliteConnection m_dbConnection;
            m_dbConnection = new SqliteConnection(connectionString);
            m_dbConnection.Open();

            string updateQuery = String.Format("UPDATE MonitoringAgentCache SET Sync = 1 WHERE Id = {0}", id);

            SqliteCommand updateSQL = new SqliteCommand(updateQuery, m_dbConnection);

            try
            {
                updateSQL.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            m_dbConnection.Close();
        }

        public static List<string> GetSyncInfo(string fileName)
        {
            string dbValues1 = null, dbValues2 = null;
            List<string> returnList = new List<string>();

            string connectionString = string.Format("Data Source={0};Version=3;", fileName);
            SqliteConnection m_dbConnection;
            m_dbConnection = new SqliteConnection(connectionString);
            m_dbConnection.Open();

            using (SqliteCommand cmd = m_dbConnection.CreateCommand())
            {
                cmd.CommandText = @"SELECT COUNT(Result) FROM MonitoringAgentCache";
                cmd.CommandType = CommandType.Text;
                SqliteDataReader r = cmd.ExecuteReader();
                while (r.Read())
                {
                    dbValues1 = (Convert.ToString(r["FileName"]));
                }
            }

            using (SqliteCommand cmd = m_dbConnection.CreateCommand())
            {
                cmd.CommandText = @"SELECT COUNT(Result) FROM MonitoringAgentCache WHERE sync = 0";
                cmd.CommandType = CommandType.Text;
                SqliteDataReader r = cmd.ExecuteReader();
                while (r.Read())
                {
                    dbValues2 = (Convert.ToString(r["FileName"]));
                }
            }
            m_dbConnection.Close();

            returnList.Add(dbValues1.ToString());
            returnList.Add(dbValues2.ToString());

            return returnList;
        }

    }
}
