using PeterKottas.DotNetCore.WindowsService;

namespace MonitoringAgent
{
    public class ConfigureService //: MicroService, IMicroService
    {
        public void Start()
        {
            //Console.WriteLine("I started");
            ServiceRunner<AgentService>.Run(config =>
            {
                var name = config.GetDefaultName();
                config.Service(serviceConfig =>
                {
                    serviceConfig.ServiceFactory((extraArguments) =>
                    {
                        return new AgentService();
                    });
                    serviceConfig.OnStart((service, extraArguments) =>
                    {
                        //Console.WriteLine("Service {0} started", name);
                        service.Start();
                    });

                    serviceConfig.OnStop(service =>
                    {
                       // Console.WriteLine("Service {0} stopped", name);
                        service.Stop();
                    });

                    serviceConfig.OnError(e =>
                    {
                        //Console.WriteLine("Service {0} errored with exception : {1}", name, e.Message);
                    });
                });
                config.SetName("TPOMM.MonitoringAgentService");
                config.SetDisplayName("TPOMM.MonitoringAgentService");
                config.SetDescription("TPOMM Monitoring Agent Service for resource checking");
            });
        }

        public void Stop()
        {
           // Console.WriteLine("I stopped");
        }
    }
    /*internal static class ConfigureService
    {
        internal static void Configure()
        {
            HostFactory.Run(configure =>
            {
                configure.Service<AgentService>(service =>
                {
                    service.ConstructUsing(s => new AgentService());
                    service.WhenStarted(s => s.Start());
                    service.WhenStopped(s => s.Stop());
                });
                //Setup Account that window service use to run.  
                configure.RunAsLocalSystem();
                configure.SetServiceName("TPOMM.MonitoringAgentService");
                configure.SetDisplayName("TPOMM.MonitoringAgentService");
                configure.SetDescription("TPOMM Monitoring Agent Service for resource checking");
            });
        }
    }*/
}