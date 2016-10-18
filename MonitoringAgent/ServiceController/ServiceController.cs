using System;
using System.Collections.Generic;
using System.ServiceProcess;

namespace ServiceControl
{
    public class ServiceControl
    {
        public void Output()
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

            CheckService(services);

        }
        public void CheckService(List<string> services)
        {
            foreach (var item in services)
            {
                ServiceController sc = new ServiceController(item.ToString());
                Console.WriteLine("{1} status:\t\t {0}", sc.Status.ToString(), item.ToString());
            }
            
        }

    }
}
