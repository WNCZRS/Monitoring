﻿using System;
using System.IO;
using System.Data.SQLite;
using System.Data;
using System.Collections.Generic;

namespace MonitoringAgent
{
    public class SQLiteDB
    {
        public static void CreateDbFile(string fileName)
        {
            if (!File.Exists(fileName))
            {
                SQLiteConnection.CreateFile(fileName);
            }
        }

        public static void CreateTable(string fileName)
        {
            string connectionString = string.Format("Data Source={0};Version=3;", fileName);
            SQLiteConnection m_dbConnection;
            m_dbConnection = new SQLiteConnection(connectionString);
            m_dbConnection.Open();

            string sql = "CREATE TABLE IF NOT EXISTS [MonitoringAgentCache] ([Id] INTEGER PRIMARY KEY, [Datetime] DATETIME, [Result] TEXT, [Sync] BOOL)";
            SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
            command.ExecuteNonQuery();

            m_dbConnection.Close();
        }

        public static void InsertToDb(string fileName, string result)
        {
            string connectionString = string.Format("Data Source={0};Version=3;", fileName);
            SQLiteConnection m_dbConnection;
            m_dbConnection = new SQLiteConnection(connectionString);
            m_dbConnection.Open();

            string insertQuery = String.Format("INSERT INTO MonitoringAgentCache (Datetime, Result, Sync) VALUES (DATETIME('NOW'), '{0}', '{1}')", result, false);

            SQLiteCommand insertSQL = new SQLiteCommand(insertQuery, m_dbConnection);

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
            SQLiteConnection m_dbConnection;
            m_dbConnection = new SQLiteConnection(connectionString);
            m_dbConnection.Open();

            using (SQLiteCommand fmd1 = m_dbConnection.CreateCommand())
            {
                fmd1.CommandText = @"SELECT Id, Result FROM MonitoringAgentCache WHERE Sync = 0 ORDER BY Datetime";
                fmd1.CommandType = CommandType.Text;
                SQLiteDataReader r = fmd1.ExecuteReader();
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
            SQLiteConnection m_dbConnection;
            m_dbConnection = new SQLiteConnection(connectionString);
            m_dbConnection.Open();

            string updateQuery = String.Format("UPDATE MonitoringAgentCache SET Sync = 1 WHERE Id = {0}", id);

            SQLiteCommand updateSQL = new SQLiteCommand(updateQuery, m_dbConnection);

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
            SQLiteConnection m_dbConnection;
            m_dbConnection = new SQLiteConnection(connectionString);
            m_dbConnection.Open();

            using (SQLiteCommand fmd1 = m_dbConnection.CreateCommand())
            {
                fmd1.CommandText = @"SELECT COUNT(Result) FROM MonitoringAgentCache";
                fmd1.CommandType = CommandType.Text;
                SQLiteDataReader r = fmd1.ExecuteReader();
                while (r.Read())
                {
                    dbValues1 = (Convert.ToString(r["FileName"]));
                }
            }

            using (SQLiteCommand fmd2 = m_dbConnection.CreateCommand())
            {
                fmd2.CommandText = @"SELECT COUNT(Result) FROM MonitoringAgentCache WHERE sync = 0";
                fmd2.CommandType = CommandType.Text;
                SQLiteDataReader r = fmd2.ExecuteReader();
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
