using System;
using System.Collections.Generic;
using System.ServiceProcess;

namespace ServiceControl
{
    public class ServiceControl
    {
        public Dictionary<string, string> Output()
        {
            Dictionary<string, string> dic = new Dictionary<string,string>();
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

            foreach (var service in services)
            {
                ServiceController sc = new ServiceController(service.ToString());
                if (sc != null)
                {
                    dic.Add(sc.ServiceName.ToString(), sc.Status.ToString());
                }
            }

            return dic; 

            //CheckService(services);

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
