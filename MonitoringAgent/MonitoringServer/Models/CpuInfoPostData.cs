
namespace MonitoringServer.Models
{
    public class CpuInfoPostData
    {
        public string MachineName { get; set; }

        public double Processor { get; set; }

        public ulong MemUsage { get; set; }

        public ulong TotalMemory { get; set; }
    }

    public class PluginOutput
    {
        //public string PluginName { get; set; }

        public string PropertyName { get; set; }

        public object Value{ get; set; }
    }
}