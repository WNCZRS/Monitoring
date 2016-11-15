using System;
using System.Collections.Generic;
using System.ServiceProcess;

namespace PluginsCollection
{
    public class ServiceControl : IPlugin
    {
        PluginOutputCollection _pluginOutputs;

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

            _pluginOutputs.PluginOuputList.Clear();

            foreach (var service in services)
            {
                ServiceController sc = new ServiceController(service.ToString());
                if (sc != null)
                {
                    serviceName = service.ToString();
                    serviceStatus = sc.Status.ToString();
                    _pluginOutputs.NewPluginOutput(serviceName, serviceStatus);
                }
            }
            return _pluginOutputs;
        }
    }
}
