using System.Web.Http;
using Microsoft.AspNet.SignalR;
using MonitoringServer.Hubs;
using PluginsCollection;
using Newtonsoft.Json;
using System.Net;

namespace MonitoringServer.Api
{
    public class PluginController : ApiController
    {
        string _connectionString = "Data Source=C:\\Users\\Marko\\Downloads\\sqlitestudio-3.1.1\\SQLiteStudio\\MonitorDB;Version=3";

        [HttpPost]
        public void Post(ClientOutput pluginOutput)
        {
            string json = JsonConvert.SerializeObject(pluginOutput.CollectionList);
           /* try
            {
                using (SQLiteConnection dbConnection = new SQLiteConnection(_connectionString))
                {
                    SQLiteCommand cmd = new SQLiteCommand("INSERT INTO test (RecCreated, ComputerID, ComputerName, JSON) VALUES (@RecCreated, @ComputerID, @ComputerName, @JSON)", dbConnection);
                    cmd.Parameters.AddWithValue("@RecCreated", DateTime.Now);
                    cmd.Parameters.AddWithValue("@ComputerID", pluginOutput.ID);
                    cmd.Parameters.AddWithValue("@ComputerName", pluginOutput.PCName);
                    cmd.Parameters.AddWithValue("@JSON", json);

                    dbConnection.Open();
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }*/

            var context = GlobalHost.ConnectionManager.GetHubContext<PluginInfo>();
            context.Clients.All.pluginsMessage(pluginOutput);
        }

        [HttpGet]
        public HttpStatusCode Get()
        {
            return HttpStatusCode.OK;
        }
    }
}
