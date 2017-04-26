using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.ServiceProcess;
using System.Xml;

namespace PluginsCollection
{
    public class ServiceControl : IPlugin
    {
        PluginOutputCollection _pluginOutputs;
        private static readonly log4net.ILog _log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public Guid UID
        {
            get
            {
                return new Guid("2d5a4274-5c67-477e-bd77-7d8d9a4d7090");
            }
        }

        public string Name
        {
            get
            {
                return "Service status";
            }
        }
        public ServiceControl()
        {
            _pluginOutputs = new PluginOutputCollection();
            _pluginOutputs.PluginUID = UID;
            _pluginOutputs.PluginName = Name;
        }

        public PluginOutputCollection Output()
        {
            List<string> services = LoadServicesFromConfig();
            _pluginOutputs.PluginOutputList.Clear();

            if (services == null)
            {
                _log.Error("No services loaded");
            }

            foreach (var service in services)
            {
                ServiceController sc = new ServiceController(service.ToString());
                List<SimplePluginOutput> listSPO = new List<SimplePluginOutput>();

                if (sc != null)
                {
                    try
                    {
                        string serviceStatus = sc.Status.ToString();
                        if (sc.Status == ServiceControllerStatus.Stopped)
                        {
                            listSPO.Add(new SimplePluginOutput(serviceStatus, true));
                        }
                        else
                        {
                            listSPO.Add(new SimplePluginOutput(serviceStatus, false));
                        }
                    }
                    catch (Exception ex)
                    {
                        _log.Warn($"Service: {service} is unavailable");
                        listSPO.Add(new SimplePluginOutput("Unavailable", false));
                    }
                    _pluginOutputs.PluginOutputList.Add(new PluginOutput(service.ToString(), listSPO));
                }
            }
            return _pluginOutputs;
        }

        private List<string> LoadServicesFromConfig()
        {
            List<string> services = new List<string>();
            XmlDocument xmlDoc = new XmlDocument();

            try
            {
                xmlDoc.Load($"{Directory.GetCurrentDirectory()}\\pluginsConfig.xml");
                XmlNode node = xmlDoc.DocumentElement.SelectSingleNode("/pluginSettings/ServiceControl/ListOfServiseToCheck");

                foreach (XmlNode item in node.ChildNodes)
                {
                    if (item.Name == "service")
                    {
                        services.Add(item.InnerText);    
                    }
                }
            }
            catch (Exception ex)
            {
                _log.Error($"Load from xml configuration failed: {ex.Message} {Environment.NewLine} {ex.StackTrace}");
            }
            return services;
        }
    }
}
