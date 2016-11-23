using System;
using System.Collections.Generic;
using System.ServiceProcess;

namespace PluginsCollection
{
    public class ServiceControl : IPlugin
    {
        PluginOutputCollection _pluginOutputs;
        private static readonly log4net.ILog _log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public string Name
        {
            get
            {
                return "Service status";
            }
        }
        public ServiceControl()
        {
            _pluginOutputs = new PluginOutputCollection(Name);
        }

        public PluginOutputCollection Output()
        {
            List<String> services = new List<string>();
            services.Add("DRS.CaseService");
            services.Add("TPNIServer");
            services.Add("TPNI.Services");
            services.Add("TPOMM.LogService");
            services.Add("MSSQLSERVER");
            services.Add("SQLSERVERAGENT");
            services.Add("ReportServer");
            services.Add("OpenVPNService");
            services.Add("MpsSvc");      
            services.Add("IISADMIN");  

            string serviceName, serviceStatus = string.Empty;

            _pluginOutputs.PluginOutputList.Clear();

            foreach (var service in services)
            {
                ServiceController sc = new ServiceController(service.ToString());
                if (sc != null)
                {
                    try
                    {
                        serviceName = service.ToString();
                        serviceStatus = sc.Status.ToString();
                        _pluginOutputs.NewPluginOutput(serviceName, serviceStatus);
                    }
                    catch (Exception ex)
                    {
                        //_log.Error(String.Format("Exeption in service: {0} \n {1}", service, ex));
                        continue;
                    }

                }
            }
            return _pluginOutputs;
        }
    }
}
