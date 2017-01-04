
using System.ComponentModel.DataAnnotations;

namespace MonitoringServer.Models
{
    public class PluginOutput
    {
        public string PluginName { get; set; }

        public string PropertyName { get; set; }

        public object Value{ get; set; }
    }
}