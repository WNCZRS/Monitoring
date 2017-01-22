using System;
using System.IO;
using System.Data.SQLite;
using System.Data;
using System.Collections.Generic;
using PluginsCollection;
using Newtonsoft.Json;

namespace MonitoringServer.Controllers
{
    public class SQLiteController
    {
        public static void CreateDbFile(string fileName)
        {
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

        public static void CreateTables(string fileName)
        {
            string connectionString = string.Format("Data Source={0};Version=3;", fileName);
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
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static void SaveBasicInfo(string fileName, ClientOutput clientOutput)
        {
            string connectionString = string.Format("Data Source={0};Version=3;", fileName);

            try
            {
                using (SQLiteConnection dbConnection = new SQLiteConnection(connectionString))
                {
                    dbConnection.Open();
                    SQLiteCommand cmd = new SQLiteCommand(@"INSERT INTO Machines (RecCreated, ComputerID, ComputerName, Customer) 
                                                            SELECT @RecCreated, @ComputerID, @ComputerName, @Customer
                                                            WHERE NOT EXISTS(SELECT ComputerID, ComputerName, Customer FROM Machines 
                                                                            where ComputerID = @ComputerID and ComputerName = @ComputerName and Customer = @Customer LIMIT 1)", dbConnection);
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

        public static void JSONToSQL(string fileName, ClientOutput clientOutput)
        {
            string json;
            string connectionString = string.Format("Data Source={0};Version=3;", fileName);

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

        public static ClientOutput JSONFromSQL(string fileName, string customer, string computerID, string computerName)
        {
            string connectionString = string.Format("Data Source={0};Version=3;", fileName);
            ClientOutput clientOutput = new ClientOutput(computerName, computerID, customer);

            try
            {
                using (SQLiteConnection dbConnection = new SQLiteConnection(connectionString))
                {
                    dbConnection.Open();
                    SQLiteCommand cmd = new SQLiteCommand("SELECT ComputerName, JSON FROM MonitoringServerStorage WHERE Customer = @Customer and ComputerID = @ComputerID ORDER BY RecCreated desc LIMIT 1", dbConnection);
                    cmd.Parameters.AddWithValue("@Customer", customer);
                    cmd.Parameters.AddWithValue("@ComputerID", computerID);

                    SQLiteDataReader reader = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                    while (reader.Read())
                    {
                        clientOutput.PCName = reader.GetString(0);
                        clientOutput.CollectionList = JsonConvert.DeserializeObject<List<PluginOutputCollection>>(reader.GetString(1));
                    }
                }
            }
            catch (Exception ex)
            {
                return null;
                throw ex;
            }
            return clientOutput;
        }
    }
}