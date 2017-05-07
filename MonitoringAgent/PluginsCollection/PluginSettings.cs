using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PluginsCollection
{
    public struct HTMLPosition
    {
        public readonly int Top;
        public readonly int Left;

        public HTMLPosition(int top, int left)
        {
            Top = top;
            Left = left;
        }
    }

    public enum PluginType
    {
        Unknown,
        Table,
        Graph
    }

    public class PluginSettings
    {
        public Guid PluginUID { get; set; }
        public string PluginName { get; set; }
        public string ComputerID { get; set; }
        public string ComputerName { get; set; }
        public string GroupName { get; set; }
        public PluginType PluginType { get; set; }
        public bool Show { get; set; }
        public int RefreshInterval { get; set; }
        public HTMLPosition HTMLPosition { get; set; }
        public double CriticalValueLimit { get; set; }
        public double WarningValueLimit { get; set; }
    }
}
