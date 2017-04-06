using System;
using System.IO;
using System.Data.SQLite;
using System.Data;
using System.Collections.Generic;
using PluginsCollection;
using Newtonsoft.Json;
using System.Configuration;

namespace MonitoringServer.Controllers
{
    public class SQLiteController
    {
        public static void CreateDbFile()
        {
            string fileName = ConfigurationManager.AppSettings["DatabasePath"];

            if (!File.Exists(fileName))
            {
                try
                {
                    SQLiteConnection.CreateFile(fileName);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }

        public static void CreateTables()
        {
            string connectionString = string.Format("Data Source={0};Version=3;", ConfigurationManager.AppSettings["DatabasePath"]);
            try
            {
                using (SQLiteConnection dbConnection = new SQLiteConnection(connectionString))
                {
                    dbConnection.Open();
                    SQLiteCommand cmd = new SQLiteCommand(@"
                        CREATE TABLE IF NOT EXISTS [MonitoringServerStorage] ([RecID] INTEGER PRIMARY KEY ASC ON CONFLICT FAIL AUTOINCREMENT NOT NULL, [RecCreated] DATETIME, [ComputerID] VARCHAR(32), [ComputerName] VARCHAR(32), [Customer] VARCHAR(32), [JSON] TEXT)", dbConnection);
                    cmd.ExecuteNonQuery();

                    cmd = new SQLiteCommand(@"CREATE TABLE IF NOT EXISTS [Machines] ([RecID] INTEGER PRIMARY KEY ASC ON CONFLICT FAIL AUTOINCREMENT NOT NULL, [RecCreated] DATETIME, [ComputerID] VARCHAR(32), [ComputerName] VARCHAR(32), [Customer] VARCHAR(32))", dbConnection);
                    cmd.ExecuteNonQuery();

                    cmd = new SQLiteCommand(@"CREATE UNIQUE INDEX IF NOT EXISTS Computer_Unique_ID ON Machines (ComputerID ASC);",
                        dbConnection);
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static List<ClientOutput> GetBasicInfo()
        {
            List<ClientOutput> clientOutputList = new List<ClientOutput>();
            string connectionString = string.Format("Data Source={0};Version=3;", ConfigurationManager.AppSettings["DatabasePath"]);

            try
            {
                using (SQLiteConnection dbConnection = new SQLiteConnection(connectionString))
                {
                    dbConnection.Open();
                    SQLiteCommand cmd = new SQLiteCommand(@"SELECT ComputerName, ComputerID, Customer FROM Machines", dbConnection);

                    SQLiteDataReader reader = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                    while (reader.Read())
                    {
                        ClientOutput co = new ClientOutput(reader.GetString(0), reader.GetString(1), reader.GetString(2));
                        clientOutputList.Add(co);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return clientOutputList;
        }

        public static void SaveBasicInfo(ClientOutput clientOutput)
        {
            string connectionString = string.Format("Data Source={0};Version=3;", ConfigurationManager.AppSettings["DatabasePath"]);

            try
            {
                using (SQLiteConnection dbConnection = new SQLiteConnection(connectionString))
                {
                    dbConnection.Open();
                    SQLiteCommand cmd = new SQLiteCommand(@"INSERT OR REPLACE INTO Machines (RecCreated, ComputerID, ComputerName, Customer) 
                                                            SELECT @RecCreated, @ComputerID, @ComputerName, @Customer", dbConnection);
                    cmd.Parameters.AddWithValue("@RecCreated", DateTime.Now);
                    cmd.Parameters.AddWithValue("@ComputerID", clientOutput.ID);
                    cmd.Parameters.AddWithValue("@ComputerName", clientOutput.PCName);
                    cmd.Parameters.AddWithValue("@Customer", clientOutput.Customer);

                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static void JSONToSQL(ClientOutput clientOutput)
        {
            string json;
            string connectionString = string.Format("Data Source={0};Version=3;", ConfigurationManager.AppSettings["DatabasePath"]);

            try
            {
                json = JsonConvert.SerializeObject(clientOutput.CollectionList);
                using (SQLiteConnection dbConnection = new SQLiteConnection(connectionString))
                {
                    dbConnection.Open();
                    SQLiteCommand cmd = new SQLiteCommand(@"INSERT INTO MonitoringServerStorage (RecCreated, ComputerID, ComputerName, Customer, JSON) VALUES (@RecCreated, @ComputerID, @ComputerName, @Customer, @JSON)", dbConnection);
                    cmd.Parameters.AddWithValue("@RecCreated", DateTime.Now);
                    cmd.Parameters.AddWithValue("@ComputerID", clientOutput.ID);
                    cmd.Parameters.AddWithValue("@ComputerName", clientOutput.PCName);
                    cmd.Parameters.AddWithValue("@Customer", clientOutput.Customer);
                    cmd.Parameters.AddWithValue("@JSON", json);

                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static ClientOutput JSONFromSQL(string customer, string computerID, string computerName)
        {
            string connectionString = string.Format("Data Source={0};Version=3;", ConfigurationManager.AppSettings["DatabasePath"]);
            ClientOutput clientOutput = new ClientOutput(computerName, computerID, customer);

            try
            {
                using (SQLiteConnection dbConnection = new SQLiteConnection(connectionString))
                {
                    dbConnection.Open();
                    SQLiteCommand cmd = new SQLiteCommand(@"SELECT ComputerName, JSON, RecCreated FROM MonitoringServerStorage 
                                                            WHERE Customer = @Customer and ComputerID = @ComputerID ORDER BY RecCreated desc LIMIT 1", dbConnection);
                    cmd.Parameters.AddWithValue("@Customer", customer);
                    cmd.Parameters.AddWithValue("@ComputerID", computerID);

                    SQLiteDataReader reader = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                    while (reader.Read())
                    {
                        clientOutput.PCName = reader.GetString(0);
                        clientOutput.CollectionList = JsonConvert.DeserializeObject<List<PluginOutputCollection>>(reader.GetString(1));
                        clientOutput.LastUpdate = reader.GetDateTime(2);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return clientOutput;
        }

        public static List<ClientOutput> LastValuesFromDB()
        {
            List<ClientOutput> clientOutputList = new List<ClientOutput>();
            string connectionString = string.Format("Data Source={0};Version=3;", ConfigurationManager.AppSettings["DatabasePath"]);
            int minutesBack = 10;
            try
            {
                using (SQLiteConnection dbConnection = new SQLiteConnection(connectionString))
                {
                    dbConnection.Open();
                    SQLiteCommand cmd = new SQLiteCommand(@"SELECT DISTINCT ComputerName, ComputerID, Customer, JSON FROM MonitoringServerStorage where RecCreated > DATETIME('now', '-' || @minutesBack || ' minutes', 'localtime') and ComputerName is not NULL and ComputerName not in ('') GROUP BY ComputerName ORDER BY RecCreated desc", dbConnection);
                    cmd.Parameters.AddWithValue("@minutesBack", minutesBack);

                    SQLiteDataReader reader = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                    while (reader.Read())
                    {
                        ClientOutput co = new ClientOutput(reader.GetString(0), reader.GetString(1), reader.GetString(2));
                        co.CollectionList = JsonConvert.DeserializeObject<List<PluginOutputCollection>>(reader.GetString(3));
                        clientOutputList.Add(co);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return clientOutputList;
        }
    }
}