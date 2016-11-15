using Topshelf;
namespace MonitoringAgent
{
    internal static class ConfigureService
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
    }
}