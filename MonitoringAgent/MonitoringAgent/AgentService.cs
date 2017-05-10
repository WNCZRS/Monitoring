using System;
using System.Net;
using System.Threading;
using Newtonsoft.Json;
using System.Collections.Generic;
using PluginsCollection;
using System.Configuration;
using System.Net.NetworkInformation;
using System.Linq;
using log4net;
using Microsoft.AspNet.SignalR.Client;
//using System.IO;
//using MySolution.Configurator.Firewall;
//using NetFwTypeLib;
using System.Management;

namespace MonitoringAgent
{
    public class AgentService 
    {
        // Logging initialization
        private static readonly ILog _log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        static PluginLoader _plugins;
        static bool _running = true;
        
        public string _dbName = ConfigurationManager.AppSettings["DatabasePath"];

        public void Start()
        {
            // Application Running Information
            _log.Info("Application is RUNNING");

            // Load Plugins
            _plugins = new PluginLoader();
            _plugins.Load();

            StartThread();
            // write code here that runs when the Windows Service starts up.  
        }

        public void Stop()
        {
            // write code here that runs when the Windows Service stops.  
        }

        private void StartThread()
        {
            Thread pollingThread = null;

            /*var application = new NetFwAuthorizedApplication()
            {
                Name = "MonitoringAgent",
                Enabled = true,
                RemoteAddresses = "*",
                Scope = NET_FW_SCOPE_.NET_FW_SCOPE_ALL,
                IpVersion = NET_FW_IP_VERSION_.NET_FW_IP_VERSION_ANY,
                ProcessImageFileName = "MonitoringAgent.exe",
            };

            Exception exception;
            int t =  (FirewallUtilities.AddApplication(application, out exception) ? 0 : -1);


              */
            try
            {
                // Create a new thread to start polling and sending the data
                pollingThread = new Thread(new ThreadStart(RunPollingThread));
                pollingThread.Start();

                Console.WriteLine("Starting thread..");
                _log.Info("Starting thread...");

                pollingThread.Join(5000);

            }
            catch (Exception)
            {
                pollingThread.Abort();
                throw;
            }

            // Wait for key
            Console.ReadLine();
        }

        private void RunPollingThread()
        {
            // Convert the object that was passed in
            DateTime lastPollTime = DateTime.MinValue;

            Console.WriteLine("Started polling...");
            _log.Info("Started polling...");

            CreateDataBaseAndTable();

            //hubConnection.Received += data => Console.WriteLine(data);

            TakeAndPostData(true);
            Thread.Sleep(1000);
            TakeAndPostData(true);

            // Start the polling loop
            while (_running)
            {
                // Poll every 5 second
                if ((DateTime.Now - lastPollTime).TotalMilliseconds >= 500)
                {
                    TakeAndPostData();
                    
                    // Reset the poll time
                    lastPollTime = DateTime.Now;
                }
                else
                {
                    Thread.Sleep(10);
                }
            } 
        }

        private void TakeAndPostData(bool initPost = false)
        {
            string json;
            string serverIP = ConfigurationManager.AppSettings["ServerIP"];
            string groupName = ConfigurationManager.AppSettings["GroupName"];
            ClientOutput output = new ClientOutput(getPCName(), getCPUID(), groupName);

            PluginOutputCollection plugOutput = new PluginOutputCollection();

            json = string.Empty;
            output.CollectionList.Clear();

            if (!initPost)
            {
                foreach (var plugin in _plugins.PluginList)
                {
                    plugOutput = plugin.Output();
                    if (plugOutput != null)
                    {
                        output.CollectionList.Add(plugOutput);
                    }
                }
            }
            else
            {
                output.InitPost = true;
            }

            if (!SendPluginOutput(output))
            {
                SaveOutputToDB(JsonConvert.SerializeObject(output));
                return;
            }

            try
            {
                Dictionary<int, string> dbValues = new Dictionary<int, string>();
                dbValues = SQLiteDB.GetStoredJson(_dbName);
                foreach (var item in dbValues)
                {
                    ClientOutput clientOutputDB = JsonConvert.DeserializeObject<ClientOutput>(item.Value);
                    if (SendPluginOutput(output))
                    {
                        SQLiteDB.UpdateStatus(_dbName, item.Key);
                    }
                }
            }
            catch (Exception err)
            {
                _log.Error("Read stored values from database", err);
                return;
            }
            
        }

        private bool SendPluginOutput(ClientOutput output)
        {
            bool sended = true;
            string serverIP = ConfigurationManager.AppSettings["ServerIP"];
            HubConnection hubConnection = new HubConnection(serverIP);
            IHubProxy monitoringHub = hubConnection.CreateHubProxy("MyHub");

            try
            {
                if (hubConnection.State == ConnectionState.Disconnected)
                {
                    hubConnection.Start().ContinueWith(task =>
                    {
                        if (task.IsFaulted)
                        {
                            Console.WriteLine($"There was an error opening the connection: {task.Exception.GetBaseException()}");
                            sended = false;
                        }
                        else
                        {
                            Console.WriteLine("Connected");
                        }
                    }).Wait();
                }

                monitoringHub.Invoke<ClientOutput>("SendPluginOutput", output).Wait();
            }
            catch (Exception ex)
            {
                _log.Error("Upload string Exception: ", ex);
                sended = false;
                throw ex;                
            }
            finally
            {
                //hubConnection.Stop();
            }
            return sended;
        }

        private void CreateDataBaseAndTable()
        {
            SQLiteDB.CreateDbFile(_dbName);
            SQLiteDB.CreateTable(_dbName);
        }

        private void SaveOutputToDB(string json)
        {
            SQLiteDB.InsertToDb(_dbName, json);

            _log.Warn("Server unreachable, writing into local db");
        }

        public string getMACAddress()
        {
            NetworkInterface[] NI = NetworkInterface.GetAllNetworkInterfaces();
            NetworkInterface ni = NI.FirstOrDefault(x => x.OperationalStatus == OperationalStatus.Up);
            return ni.GetPhysicalAddress().ToString();
        }

        public string getPCName()
        {
            string pcName = ConfigurationManager.AppSettings["PCName"];

            if (pcName != null && pcName != string.Empty)
            {
                return pcName;
            }
            else
            {
                return Environment.MachineName;
            }
        }

        public string getCPUID()
        {
            string cpuID = string.Empty;
            ManagementClass mc = new ManagementClass("win32_processor");
            ManagementObjectCollection moc = mc.GetInstances();

            foreach (ManagementObject mo in moc)
            {
                cpuID = mo.Properties["processorID"].Value.ToString();
            }
            return cpuID;
        }


    }
}

  /*
namespace MySolution.Configurator.Firewall
{
    using System;
    using System.Linq;
    using NetFwTypeLib;

    public sealed class NetFwAuthorizedApplication :
        INetFwAuthorizedApplication
    {
        public string Name { get; set; }
        public bool Enabled { get; set; }
        public NET_FW_SCOPE_ Scope { get; set; }
        public string RemoteAddresses { get; set; }
        public string ProcessImageFileName { get; set; }
        public NET_FW_IP_VERSION_ IpVersion { get; set; }

        public NetFwAuthorizedApplication()
        {
            this.Name = "";
            this.Enabled = false;
            this.RemoteAddresses = "";
            this.ProcessImageFileName = "";
            this.Scope = NET_FW_SCOPE_.NET_FW_SCOPE_ALL;
            this.IpVersion = NET_FW_IP_VERSION_.NET_FW_IP_VERSION_ANY;
        }

        public NetFwAuthorizedApplication(string name, bool enabled, string remoteAddresses, NET_FW_SCOPE_ scope, NET_FW_IP_VERSION_ ipVersion, string processImageFileName)
        {
            this.Name = name;
            this.Scope = scope;
            this.Enabled = enabled;
            this.IpVersion = ipVersion;
            this.RemoteAddresses = remoteAddresses;
            this.ProcessImageFileName = processImageFileName;
        }

        public static NetFwAuthorizedApplication FromINetFwAuthorizedApplication(INetFwAuthorizedApplication application)
        {
            return (new NetFwAuthorizedApplication(application.Name, application.Enabled, application.RemoteAddresses, application.Scope, application.IpVersion, application.ProcessImageFileName));
        }
    }
}      */
  /*
namespace MySolution.Configurator.Firewall
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using NetFwTypeLib;
    using System.Runtime.InteropServices;

    public static class FirewallUtilities
    {
        public static bool GetApplication(string processImageFileName, out INetFwAuthorizedApplication application, out Exception exception)
        {
            var result = false;
            var comObjects = new Stack<object>();

            exception = null;
            application = null;

            if (processImageFileName == null) { throw (new ArgumentNullException("processImageFileName")); }
            if (processImageFileName.Trim().Length == 0) { throw (new ArgumentException("The argument [processImageFileName] cannot be empty.", "processImageFileName")); }

            try
            {
                var type = Type.GetTypeFromProgID("HNetCfg.FwMgr", true);

                try
                {
                    var manager = (INetFwMgr)Activator.CreateInstance(type);
                    comObjects.Push(manager);

                    try
                    {
                        var policy = manager.LocalPolicy;
                        comObjects.Push(policy);

                        var profile = policy.CurrentProfile;
                        comObjects.Push(profile);

                        var applications = profile.AuthorizedApplications;
                        comObjects.Push(applications);

                        foreach (INetFwAuthorizedApplication app in applications)
                        {
                            comObjects.Push(app);

                            if (string.Compare(app.ProcessImageFileName, processImageFileName, true, CultureInfo.InvariantCulture) == 0)
                            {
                                result = true;
                                application = NetFwAuthorizedApplication.FromINetFwAuthorizedApplication(app);

                                break;
                            }
                        }

                        if (!result) { throw (new Exception("The requested application was not found.")); }
                    }
                    catch (Exception e)
                    {
                        exception = e;
                    }
                }
                catch (Exception e)
                {
                    exception = e;
                }
                finally
                {
                    while (comObjects.Count > 0)
                    {
                        Marshal.ReleaseComObject(comObjects.Pop());
                        //ComUtilities.ReleaseComObject(comObjects.Pop());
                    }
                }
            }
            catch (Exception e)
            {
                exception = e;
            }
            finally
            {
            }

            return (result);
        }

        public static bool AddApplication(INetFwAuthorizedApplication application, out Exception exception)
        {
            var result = false;
            var comObjects = new Stack<object>();

            exception = null;

            if (application == null) { throw (new ArgumentNullException("application")); }

            try
            {
                var type = Type.GetTypeFromProgID("HNetCfg.FwMgr", true);

                try
                {
                    var manager = (INetFwMgr)Activator.CreateInstance(type);
                    comObjects.Push(manager);

                    try
                    {
                        var policy = manager.LocalPolicy;
                        comObjects.Push(policy);

                        var profile = policy.CurrentProfile;
                        comObjects.Push(profile);

                        var applications = profile.AuthorizedApplications;
                        comObjects.Push(applications);

                        applications.Add(application);

                        result = true;
                    }
                    catch (Exception e)
                    {
                        exception = e;
                    }
                }
                catch (Exception e)
                {
                    exception = e;
                }
                finally
                {
                    while (comObjects.Count > 0)
                    {
                        Marshal.ReleaseComObject(comObjects.Pop());
                        //ComUtilities.ReleaseComObject(comObjects.Pop());
                    }
                }
            }
            catch (Exception e)
            {
                exception = e;
            }
            finally
            {
            }

            return (result);
        }

        public static bool RemoveApplication(string processImageFileName, out Exception exception)
        {
            var result = false;
            var comObjects = new Stack<object>();

            exception = null;

            if (processImageFileName == null) { throw (new ArgumentNullException("processImageFileName")); }
            if (processImageFileName.Trim().Length == 0) { throw (new ArgumentException("The argument [processImageFileName] cannot be empty.", "processImageFileName")); }

            try
            {
                var type = Type.GetTypeFromProgID("HNetCfg.FwMgr", true);

                try
                {
                    var manager = (INetFwMgr)Activator.CreateInstance(type);
                    comObjects.Push(manager);

                    try
                    {
                        var policy = manager.LocalPolicy;
                        comObjects.Push(policy);

                        var profile = policy.CurrentProfile;
                        comObjects.Push(profile);

                        var applications = profile.AuthorizedApplications;
                        comObjects.Push(applications);

                        applications.Remove(processImageFileName);

                        result = true;
                    }
                    catch (Exception e)
                    {
                        exception = e;
                    }
                }
                catch (Exception e)
                {
                    exception = e;
                }
                finally
                {
                    while (comObjects.Count > 0)
                    {
                        Marshal.ReleaseComObject(comObjects.Pop());
                        //ComUtilities.ReleaseComObject(comObjects.Pop());
                    }
                }
            }
            catch (Exception e)
            {
                exception = e;
            }
            finally
            {
            }

            return (result);
        }    
    }
}  */