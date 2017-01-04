using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace PluginsCollection
{
    public interface IPlugin
    {
        string Name { get; }
        PluginOutputCollection Output();
    }

    public class PluginOutput
    {
        private string  _propertyName;
        private object  _value;
        private bool    _isCritical;

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

        public bool IsCritical
        {
            get
            {
                return _isCritical;
            }
            set
            {
                _isCritical = value;
            }
        }


        public PluginOutput(string propertyName, object value, bool isCritical)
        {
            _propertyName = propertyName;
            _value = value;
            _isCritical = isCritical;
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

        public void NewPluginOutput(string name, object value, bool isCritical)
        {
            _pluginOutputList.Add(new PluginOutput(name, value, isCritical));
        }
    }

    public class ClientOutput
    {
        private List<PluginOutputCollection> _collectionList;
        private string _ID;
        private string _pcName;
        private string _customer;
        private bool _initPost;

        public List<PluginOutputCollection> CollectionList
        {
            get
            {
                return _collectionList;
            }
            set
            {
                _collectionList = value;
            }
        }

        public string ID
        {
            get
            {
                return _ID;
            }
        }

        public string PCName
        {
            get
            {
                return _pcName;
            }
            set
            {
                _pcName = value;
            }
        }

        public string Customer
        {
            get
            {
                return _customer;
            }
        }

        public bool InitPost
        {
            get
            {
                return _initPost;
            }
            set
            {
                _initPost = value;
            }
        }

        public ClientOutput(string pcName, string id, string customer)
        {
            _pcName = pcName;
            _ID = id;
            _customer = customer;
            _collectionList = new List<PluginOutputCollection>();
            _initPost = false;
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
            Type parentType = typeof(IPlugin);
            Assembly assembly = Assembly.GetExecutingAssembly();
            Type[] types = assembly.GetTypes();
            IEnumerable<Type> imp = types.Where(t => t.GetInterfaces().Contains(parentType));
            pluginList.Clear();

            foreach (Type type in imp)
            {
                pluginList.Add((IPlugin)Activator.CreateInstance(type));
            }

            return pluginList;
        }
    }
}
