using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace PluginsCollection
{
    public interface IPlugin
    {
        string PluginName { get; }
        Guid PluginUID { get; }
        PluginType PluginType { get; }
        PluginOutputCollection Output();
    }

    public class PluginOutput
    {
        public string PropertyName { get; set; }
        public List<SimplePluginOutput> Values { get; set; }

        public PluginOutput(string propertyName, List<SimplePluginOutput> values)
        {
            PropertyName = propertyName;
            Values = values;
        }
    }

    public class SimplePluginOutput
    {
        public object Value { get; set; }
        //public bool IsWarning { get; set; }
        public bool IsCritical { get; set; }
        
        //TODO: IsWarning
        public SimplePluginOutput(object value, bool isCritical/*, bool isWarning*/)
        {
            Value = value;
            //IsWarning = isWarning;
            IsCritical = isCritical;
        }
    }

    public class PluginOutputCollection : IPlugin
    {
        public string PluginName { get; set; }
        public Guid PluginUID { get; set; }
        public PluginType PluginType { get; set; }
        public List<PluginOutput> PluginOutputList { get; set; }

        public PluginOutputCollection()
        {
            PluginOutputList = new List<PluginOutput>();
        }

        public PluginOutputCollection Output()
        {
            return null;
        }
    }

    public class ClientOutput
    {
        public List<PluginOutputCollection> CollectionList { get; set; }
        public string ID { get; }
        public string PCName { get; set; }
        public string Group { get; set; }
        public bool InitPost { get; set; }
        public DateTime LastUpdate { get; set; }

        public ClientOutput(string pcName, string id, string group)
        {
            PCName = pcName;
            ID = id;
            Group = group;
            CollectionList = new List<PluginOutputCollection>();
            InitPost = false;
            LastUpdate = DateTime.MinValue;
        }

        public PluginOutputCollection GetPluginOutputByName(string pluginName)
        {
            return CollectionList.Find(item => item.PluginName == pluginName);
        }
    }

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

    public class PluginLoader
    {
        public List<IPlugin> PluginList { get; set; }

        public PluginLoader()
        {
            PluginList = new List<IPlugin>();
        }

        public void Load(string pluginsPath = "")
        {
            LoadSystemPlugins();
            LoadDllPlugins(pluginsPath);
        }

        private void LoadSystemPlugins()
        {
            try
            {
                Type[] types = Assembly.GetExecutingAssembly().GetTypes();
                IEnumerable<Type> imp = types.Where(t => t.GetInterfaces().Contains(typeof(IPlugin)));
                PluginList.Clear();

                foreach (Type type in imp)
                {
                    PluginList.Add((IPlugin)Activator.CreateInstance(type));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private void LoadDllPlugins(string pluginsPath = "")
        {
            string path = "Plugins";
            if (!string.IsNullOrWhiteSpace(pluginsPath))
            {
                path = pluginsPath;
            }

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            else
            {
                string[] dllFileNames = Directory.GetFiles(path, "*.dll");
                try
                {
                    foreach (string dllFile in dllFileNames)
                    {
                        AssemblyName an = AssemblyName.GetAssemblyName(dllFile);
                        Type[] types = Assembly.Load(an).GetTypes();
                        IEnumerable<Type> imp = types.Where(t => t.GetInterfaces().Contains(typeof(IPlugin)));
                        foreach (Type type in imp)
                        {
                            IPlugin plugin = (IPlugin)Activator.CreateInstance(type);
                            if (!PluginList.Any(item => item.PluginName == plugin.PluginName))
                            {
                                PluginList.Add(plugin);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
        }
    }
}
