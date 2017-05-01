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
        public void CreateDbFile()
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

        public void CreateTables()
        {
            string connectionString = string.Format("Data Source={0};Version=3;", ConfigurationManager.AppSettings["DatabasePath"]);
            try
            {
                using (SQLiteConnection dbConnection = new SQLiteConnection(connectionString))
                {
                    dbConnection.Open();
                    SQLiteCommand cmd = new SQLiteCommand(@"CREATE TABLE IF NOT EXISTS MonitoringServerStorage (
                                                                RecID        INTEGER      PRIMARY KEY ASC ON CONFLICT FAIL AUTOINCREMENT
                                                                                          NOT NULL,
                                                                RecCreated   DATETIME,
                                                                ComputerID   VARCHAR (32),
                                                                ComputerName VARCHAR (32),
                                                                [Group]      VARCHAR (32),
                                                                JSON         TEXT
                                                            );", dbConnection);
                    cmd.ExecuteNonQuery();

                    cmd = new SQLiteCommand(@"CREATE TABLE IF NOT EXISTS Machines (
                                                RecID        INTEGER      PRIMARY KEY ASC ON CONFLICT REPLACE AUTOINCREMENT,
                                                RecCreated   DATETIME,
                                                ComputerID   VARCHAR (32) NOT NULL,
                                                ComputerName VARCHAR (32),
                                                [Group]      VARCHAR (32),
                                                UNIQUE (
                                                    ComputerID
                                                )
                                                ON CONFLICT REPLACE
                                            );", dbConnection);
                    cmd.ExecuteNonQuery();

                    cmd = new SQLiteCommand(@"CREATE UNIQUE INDEX IF NOT EXISTS Computer_Unique_ID ON Machines (ComputerID ASC);",
                        dbConnection);
                    cmd.ExecuteNonQuery();

                    cmd = new SQLiteCommand(@"CREATE TABLE IF NOT EXISTS PluginSettings (
                                                RecID              INTEGER      PRIMARY KEY ASC ON CONFLICT REPLACE
                                                                                NOT NULL,
                                                RecModified        DATE,
                                                PluginUID          TEXT         NOT NULL
                                                                                REFERENCES Plugins (PluginUID),
                                                ComputerID         VARCHAR (32) REFERENCES Machines (RecID),
                                                ShowPlugin         BOOLEAN,
                                                RefreshInterval    INTEGER,
                                                Position_html_top  INTEGER,
                                                Position_html_left INTEGER,
                                                CriticalValueLimit NUMERIC,
                                                WarningValueLimit  NUMERIC,
                                                UNIQUE (
                                                    PluginUID,
                                                    ComputerID
                                                )
                                                ON CONFLICT REPLACE
                                            );", dbConnection);
                    cmd.ExecuteNonQuery();

                    cmd = new SQLiteCommand(@"CREATE TABLE IF NOT EXISTS Plugins (
                                                PluginUID TEXT UNIQUE ON CONFLICT FAIL
                                                               PRIMARY KEY
                                                               NOT NULL,
                                                Name      TEXT,
                                                Type      INTEGER
                                            );", dbConnection);
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void InitPlugins()
        {
            PluginLoader loader = new PluginLoader();
            string path = ConfigurationManager.AppSettings["PluginsPath"];
            loader.Load(path);
            SavePluginDefinition(loader.PluginList);

        }

        private void SavePluginDefinition(List<IPlugin> plugins)
        {
            string connectionString = string.Format("Data Source={0};Version=3;", ConfigurationManager.AppSettings["DatabasePath"]);

            try
            {
                foreach (var plugin in plugins)
                {
                    using (SQLiteConnection dbConnection = new SQLiteConnection(connectionString))
                    {
                        dbConnection.Open();
                        SQLiteCommand cmd = new SQLiteCommand(@"update Plugins
                                                                set Name = @Name,
                                                                Type = @Type                                                            
                                                                where PluginUID = @PluginUID;

                                                                INSERT INTO Plugins (PluginUID, Name, Type) 
                                                                SELECT @PluginUID, @Name, @Type                                                           
                                                                WHERE (Select Changes() = 0)",
                                                                dbConnection);
                        cmd.Parameters.AddWithValue("@Name", plugin.Name);
                        cmd.Parameters.AddWithValue("@Type", plugin.Type);
                        cmd.Parameters.AddWithValue("@PluginUID", plugin.UID.ToString());
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public List<ClientOutput> GetBasicInfo()
        {
            List<ClientOutput> clientOutputList = new List<ClientOutput>();
            string connectionString = string.Format("Data Source={0};Version=3;", ConfigurationManager.AppSettings["DatabasePath"]);

            try
            {
                using (SQLiteConnection dbConnection = new SQLiteConnection(connectionString))
                {
                    dbConnection.Open();
                    SQLiteCommand cmd = new SQLiteCommand(@"SELECT ComputerName, ComputerID, [Group] 
                                                            FROM Machines", 
                                                            dbConnection);

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

        public void SaveBasicInfo(ClientOutput clientOutput)
        {
            string connectionString = string.Format("Data Source={0};Version=3;", ConfigurationManager.AppSettings["DatabasePath"]);

            try
            {
                using (SQLiteConnection dbConnection = new SQLiteConnection(connectionString))
                {
                    dbConnection.Open();
                    /*SQLiteCommand cmd = new SQLiteCommand(@"INSERT OR REPLACE INTO Machines 
                                                            (RecCreated, ComputerID, ComputerName, [Group]) 
                                                            SELECT @RecCreated, @ComputerID, @ComputerName, @Group", 
                                                            dbConnection);    */
                    SQLiteCommand cmd = new SQLiteCommand(@"update Machines
                                                            set [Group] = @Group,
                                                            ComputerName = @ComputerName
                                                            where ComputerID = @ComputerID;

                                                            INSERT INTO Machines (RecCreated, ComputerID, ComputerName, [Group]) 
                                                            SELECT @RecCreated, @ComputerID, @ComputerName, @Group                                                           
                                                            WHERE (Select Changes() = 0);", dbConnection);
                    cmd.Parameters.AddWithValue("@RecCreated", DateTime.Now);
                    cmd.Parameters.AddWithValue("@ComputerID", clientOutput.ID);
                    cmd.Parameters.AddWithValue("@ComputerName", clientOutput.PCName);
                    cmd.Parameters.AddWithValue("@Group", clientOutput.Group);

                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void JSONToSQL(ClientOutput clientOutput)
        {
            string json;
            string connectionString = string.Format("Data Source={0};Version=3;", ConfigurationManager.AppSettings["DatabasePath"]);

            try
            {
                json = JsonConvert.SerializeObject(clientOutput.CollectionList);
                using (SQLiteConnection dbConnection = new SQLiteConnection(connectionString))
                {
                    dbConnection.Open();
                    SQLiteCommand cmd = new SQLiteCommand(@"INSERT INTO MonitoringServerStorage 
                                                            (RecCreated, ComputerID, ComputerName, [Group], JSON) VALUES 
                                                            (@RecCreated, @ComputerID, @ComputerName, @Group, @JSON)", 
                                                            dbConnection);
                    cmd.Parameters.AddWithValue("@RecCreated", DateTime.Now);
                    cmd.Parameters.AddWithValue("@ComputerID", clientOutput.ID);
                    cmd.Parameters.AddWithValue("@ComputerName", clientOutput.PCName);
                    cmd.Parameters.AddWithValue("@Group", clientOutput.Group);
                    cmd.Parameters.AddWithValue("@JSON", json);

                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public ClientOutput JSONFromSQL(string group, string computerID, string computerName)
        {
            string connectionString = string.Format("Data Source={0};Version=3;", ConfigurationManager.AppSettings["DatabasePath"]);
            ClientOutput clientOutput = new ClientOutput(computerName, computerID, group);

            try
            {
                using (SQLiteConnection dbConnection = new SQLiteConnection(connectionString))
                {
                    dbConnection.Open();
                    SQLiteCommand cmd = new SQLiteCommand(@"SELECT ComputerName, JSON, RecCreated 
                                                            FROM MonitoringServerStorage 
                                                            WHERE [Group] = @Group 
                                                            and ComputerID = @ComputerID 
                                                            ORDER BY RecCreated desc 
                                                            LIMIT 1", 
                                                            dbConnection);
                    cmd.Parameters.AddWithValue("@Group", group);
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

        public List<ClientOutput> LastValuesFromDB()
        {
            List<ClientOutput> clientOutputList = new List<ClientOutput>();
            string connectionString = string.Format("Data Source={0};Version=3;", ConfigurationManager.AppSettings["DatabasePath"]);
            int minutesBack = 10;
            try
            {
                using (SQLiteConnection dbConnection = new SQLiteConnection(connectionString))
                {
                    dbConnection.Open();
                    SQLiteCommand cmd = new SQLiteCommand(@"SELECT DISTINCT ComputerName, ComputerID, [Group], JSON 
                                                            FROM MonitoringServerStorage 
                                                            where RecCreated > DATETIME('now', '-' || @minutesBack || ' minutes', 'localtime') 
                                                            and ComputerName is not NULL 
                                                            and ComputerName not in ('') 
                                                            GROUP BY ComputerName 
                                                            ORDER BY RecCreated desc", 
                                                            dbConnection);
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

        public PluginSettings GetPluginSettings(string pluginUID/*, string ComputerID*/)
        {
            if (!string.IsNullOrWhiteSpace(pluginUID))
            {
                return GetPluginSettings(new Guid(pluginUID));
            }
            return null;
        }

        public PluginSettings GetPluginSettings(Guid pluginUID/*, string ComputerID*/)
        {
            PluginSettings pluginSettings = new PluginSettings();
            string connectionString = string.Format("Data Source={0};Version=3;", ConfigurationManager.AppSettings["DatabasePath"]);

            try
            {
                using (SQLiteConnection dbConnection = new SQLiteConnection(connectionString))
                {
                    dbConnection.Open();
                    SQLiteCommand cmd = new SQLiteCommand(@"SELECT PluginType, ShowPlugin, RefreshInterval, Position_html_top, 
                                                            Position_html_left, CriticalValueLimit, WarningValueLimit 
                                                            FROM PluginSettings 
                                                            WHERE PluginUID = @PluginUID 
                                                            LIMIT 1", 
                                                            dbConnection);
                    cmd.Parameters.AddWithValue("@PluginUID", pluginUID.ToString());

                    SQLiteDataReader reader = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                    while (reader.Read())
                    {
                        pluginSettings.PluginUID = pluginUID;
                        pluginSettings.PluginType = (PluginType)reader.GetValue(0);
                        pluginSettings.Show = reader.GetBoolean(1);
                        pluginSettings.RefreshInterval = reader.GetInt32(2);
                        pluginSettings.HTMLPosition = new HTMLPosition(reader.GetInt32(3), reader.GetInt32(4));
                        pluginSettings.CriticalValueLimit = reader.GetDouble(5);
                        pluginSettings.WarningValueLimit = reader.GetDouble(6);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return pluginSettings;
        }

        public List<PluginSettings> GetAllPluginSettings()
        {
            List<PluginSettings> pluginSettingsList = new List<PluginSettings>();
            string connectionString = string.Format("Data Source={0};Version=3;", ConfigurationManager.AppSettings["DatabasePath"]);

            try
            {
                using (SQLiteConnection dbConnection = new SQLiteConnection(connectionString))
                {
                    dbConnection.Open();
                    SQLiteCommand cmd = new SQLiteCommand(@"SELECT ps.PluginUID, p.Name, ps.ComputerID, m.ComputerName, m.[Group], p.Type, ShowPlugin, 
                                                            RefreshInterval, Position_html_top, Position_html_left, CriticalValueLimit, WarningValueLimit 
                                                            FROM PluginSettings ps
                                                            JOIN Machines m on ps.ComputerID = m.ComputerID
                                                            JOIN Plugins p on ps.PluginUID = p.PluginUID
                                                            ORDER BY m.[Group], ps.ComputerID, ps.PluginUID",
                                                            dbConnection);

                    SQLiteDataReader reader = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                    while (reader.Read())
                    {
                        PluginSettings pluginSettings = new PluginSettings();
                        pluginSettings.PluginUID = reader.GetGuid(0);
                        pluginSettings.PluginName = reader.GetString(1);
                        pluginSettings.ComputerID = reader.GetString(2);
                        pluginSettings.ComputerName = reader.GetString(3);
                        pluginSettings.GroupName = reader.GetString(4);
                        pluginSettings.PluginType = (PluginType)reader.GetInt32(5);
                        pluginSettings.Show = (reader.GetValue(6) == DBNull.Value) ? true : reader.GetBoolean(6);
                        pluginSettings.RefreshInterval = (reader.GetValue(7) == DBNull.Value) ? 5 : reader.GetInt32(7); //TODO: default value for refreshInterval 
                        pluginSettings.HTMLPosition = new HTMLPosition(reader.GetInt32(8), reader.GetInt32(9));
                        pluginSettings.CriticalValueLimit = (reader.GetValue(10) == DBNull.Value) ? Double.MaxValue : reader.GetDouble(10);
                        pluginSettings.WarningValueLimit = (reader.GetValue(11) == DBNull.Value) ? Double.MaxValue : reader.GetDouble(11);
                        pluginSettingsList.Add(pluginSettings);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return pluginSettingsList;
        }

        public void SetPluginSettings(PluginSettings pluginSettings)
        {
            string connectionString = string.Format("Data Source={0};Version=3;", ConfigurationManager.AppSettings["DatabasePath"]);

            try
            {
                using (SQLiteConnection dbConnection = new SQLiteConnection(connectionString))
                {
                    dbConnection.Open();
                    SQLiteCommand cmd = new SQLiteCommand(@"INSERT OR REPLACE INTO PluginSettings 
                                                            (RecModified, PluginUID, ComputerID, PluginType, ShowPlugin, RefreshInterval, Position_html_top,
                                                            Position_html_left, CriticalValueLimit, WarningValueLimit)
                                                            VALUES (@RecModified, @PluginUID, @ComputerID, @PluginType, @ShowPlugin, @RefreshInterval, 
                                                            @Position_html_top, @Position_html_left, @CriticalValueLimit, @WarningValueLimit)",
                                                            dbConnection);
                    cmd.Parameters.AddWithValue("@RecModified", DateTime.Now);
                    cmd.Parameters.AddWithValue("@PluginUID", pluginSettings.PluginUID);
                    cmd.Parameters.AddWithValue("@ComputerID", pluginSettings.ComputerID);
                    cmd.Parameters.AddWithValue("@PluginType", pluginSettings.PluginType);
                    cmd.Parameters.AddWithValue("@ShowPugin", pluginSettings.Show);
                    cmd.Parameters.AddWithValue("@RefreshInterval", pluginSettings.RefreshInterval);
                    cmd.Parameters.AddWithValue("@Position_html_top", pluginSettings.HTMLPosition.Top);
                    cmd.Parameters.AddWithValue("@Position_html_left", pluginSettings.HTMLPosition.Left);
                    cmd.Parameters.AddWithValue("@CriticalValueLimit", pluginSettings.CriticalValueLimit);
                    cmd.Parameters.AddWithValue("@WarningValueLimit", pluginSettings.WarningValueLimit);
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public List<PluginSettings> GetHTMLPositions(string computerID)
        {
            List<PluginSettings> positions = new List<PluginSettings>();

            string connectionString = string.Format("Data Source={0};Version=3;", ConfigurationManager.AppSettings["DatabasePath"]);

            try
            {
                using (SQLiteConnection dbConnection = new SQLiteConnection(connectionString))
                {
                    dbConnection.Open();
                    SQLiteCommand cmd = new SQLiteCommand(@"SELECT PluginUID, Position_html_top, Position_html_left 
                                                            FROM PluginSettings 
                                                            WHERE ComputerID = @ComputerID",
                                                            dbConnection);
                    cmd.Parameters.AddWithValue("@ComputerID", computerID);

                    SQLiteDataReader reader = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                    while (reader.Read())
                    {
                        PluginSettings plugSet = new PluginSettings();
                        plugSet.ComputerID = computerID;
                        plugSet.PluginUID = reader.GetGuid(0);
                        plugSet.HTMLPosition = new HTMLPosition(reader.GetInt32(1), reader.GetInt32(2));
                        positions.Add(plugSet);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return positions;
        }

        public void SaveHTMLPosition(string computerID, string pluginUID, int posTop, int posLeft)
        {
            string connectionString = string.Format("Data Source={0};Version=3;", ConfigurationManager.AppSettings["DatabasePath"]);

            try
            {
                using (SQLiteConnection dbConnection = new SQLiteConnection(connectionString))
                {
                    dbConnection.Open();
                    SQLiteCommand cmd = new SQLiteCommand(@"INSERT OR REPLACE INTO PluginSettings 
                                                            (RecModified, PluginUID, ComputerID, Position_html_top, Position_html_left)
                                                            VALUES (@RecModified, @PluginUID, @ComputerID, @Position_html_top, @Position_html_left)",
                                                            dbConnection);
                    cmd.Parameters.AddWithValue("@RecModified", DateTime.Now);
                    cmd.Parameters.AddWithValue("@PluginUID", pluginUID);
                    cmd.Parameters.AddWithValue("@ComputerID", computerID);
                    cmd.Parameters.AddWithValue("@Position_html_top", posTop);
                    cmd.Parameters.AddWithValue("@Position_html_left", posLeft);

                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public int GetMachineID(string computerID)
        {
            string connectionString = string.Format("Data Source={0};Version=3;", ConfigurationManager.AppSettings["DatabasePath"]);

            try
            {
                using (SQLiteConnection dbConnection = new SQLiteConnection(connectionString))
                {
                    dbConnection.Open();
                    SQLiteCommand cmd = new SQLiteCommand(@"SELECT RecID FROM Machines 
                                                            WHERE ComputerID = @ComputerID",
                                                            dbConnection);
                    cmd.Parameters.AddWithValue("@ComputerID", computerID);

                    return Convert.ToInt32(cmd.ExecuteScalar(CommandBehavior.CloseConnection));
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}