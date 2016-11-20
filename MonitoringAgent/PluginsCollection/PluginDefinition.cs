using System.Collections.Generic;

namespace PluginsCollection
{
    public interface IPlugin
    {
        string Name { get; }
        PluginOutputCollection Output();
    }

    public class PluginOutput
    {
        private string _propertyName;
        private object _value;

        public string PropertyName
        {
            get
            {
                return _propertyName;
            }
            set
            {
                _propertyName = value;
            }
        }
        public object Value
        {
            get
            {
                return _value;
            }
            set
            {
                _value = value;
            }
        }

        public PluginOutput(string propertyName, object value)
        {
            _propertyName = propertyName;
            _value = value;
        }
    }

    public class PluginOutputCollection
    {
        string _pluginName;
        List<PluginOutput> _pluginOutputList;

        public string PluginName
        {
            get
            {
                return _pluginName;
            }
            set
            {
                _pluginName = value;
            }
        }

        public List<PluginOutput> PluginOutputList
        {
            get
            {
                return _pluginOutputList;
            }
        }

        public PluginOutputCollection(string name = "")
        {
            _pluginName = name;
            _pluginOutputList = new List<PluginOutput>();
        }

        public void NewPluginOutput(string name, object value)
        {
            _pluginOutputList.Add(new PluginOutput(name, value));
        }
    }

    public class PluginLoader
    {
        //private string _path;

        public List<IPlugin> pluginList;

        public PluginLoader()
        {
            pluginList = new List<IPlugin>();
           // _path = "Plugins";
        }
        public List<IPlugin> Loader()
        {
            pluginList = LoadPlugins();

            return pluginList;
            /*string[] dllFileNames = null;

            if (!Directory.Exists(_path))
            {
                Directory.CreateDirectory(_path);
            }
            else
            {
                dllFileNames = Directory.GetFiles(_path, "*.dll");
            }

            //Fill the imports of this object
            try
            {
                foreach (string dllFile in dllFileNames)
                {
                    AssemblyName an = AssemblyName.GetAssemblyName(dllFile);

                    foreach (Type type in Assembly.Load(an).GetTypes())
                    {
                        IPlugin plugin = (IPlugin)Activator.CreateInstance(type);
                        pluginList.Add(plugin);
                    }
                }
                return pluginList;
            }
            catch (CompositionException compositionException)
            {
                Console.WriteLine(compositionException.ToString());
                return null;
            }   */

        }

        public List<IPlugin> LoadPlugins()
        {
            pluginList.Clear();

            pluginList.Add(new DiskSpace());
            pluginList.Add(new ServiceControl());
            pluginList.Add(new MachineIdentifier());
            pluginList.Add(new MACAddress());
            pluginList.Add(new TerminalServicesUsers());
            pluginList.Add(new ComputerName());
            //pluginList.Add(new Performance());
            //pluginList.Add(new SQLdbAvailabilityCheck());

            return pluginList;
        }
    }
}
