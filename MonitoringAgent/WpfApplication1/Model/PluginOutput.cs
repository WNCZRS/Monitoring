﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace WpfApplication1.Model
{
    public interface IPlugin
    {
        string Name { get; }
        PluginOutputCollection Output();
    }

    public class PluginOutput
    {
        private string _propertyName;
        private List<SimplePluginOutput> _values;

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

        public List<SimplePluginOutput> Values
        {
            get
            {
                return _values;
            }
            set
            {
                _values = value;
            }
        }

        public PluginOutput(string propertyName, List<SimplePluginOutput> values)
        {
            _propertyName = propertyName;
            _values = values;
        }
    }

    public class SimplePluginOutput
    {
        private object _value;
        private bool _isCritical;

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

        public SimplePluginOutput(object value, bool isCritical)
        {
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

        public void NewPluginOutput(string name, List<SimplePluginOutput> listOfSimplePluginOutput)
        {
            _pluginOutputList.Add(new PluginOutput(name, listOfSimplePluginOutput));
        }
    }

    public class ClientOutput
    {
        private List<PluginOutputCollection> _collectionList;
        private string _ID;
        private string _pcName;
        private string _customer;
        private bool _initPost;
        private DateTime _lastUpdate;

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
            set
            {
                _customer = value;
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

        public DateTime LastUpdate
        {
            get
            {
                return _lastUpdate;
            }
            set
            {
                _lastUpdate = value;
            }
        }

        public ClientOutput(string pcName, string id, string customer)
        {
            _pcName = pcName;
            _ID = id;
            _customer = customer;
            _collectionList = new List<PluginOutputCollection>();
            _initPost = false;
            _lastUpdate = DateTime.MinValue;
        }

        public PluginOutputCollection GetPluginOutputByName(string pluginName)
        {
            PluginOutputCollection poc;
            poc = this.CollectionList.Find(item => item.PluginName == pluginName);

            return poc;
        }

        public static ClientOutput GetSampleClientOutput()
        {
            ClientOutput CO = new ClientOutput("myPC", "1234", "customerTest");
            CO.CollectionList.Add(new PluginOutputCollection("pluginName"));
            CO.CollectionList.Add(new PluginOutputCollection("pluginName2"));
            List<SimplePluginOutput> listSimplePluginOutput = new List<SimplePluginOutput>();
            listSimplePluginOutput.Add(new SimplePluginOutput("value", true));
            listSimplePluginOutput.Add(new SimplePluginOutput("value2", true));
            CO.CollectionList[0].PluginOutputList.Add(new PluginOutput("propName", listSimplePluginOutput));
            CO.CollectionList[1].PluginOutputList.Add(new PluginOutput("propName2", listSimplePluginOutput));

            return CO;
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
            Type[] types = Assembly.GetExecutingAssembly().GetTypes();
            IEnumerable<Type> imp = types.Where(t => t.GetInterfaces().Contains(typeof(IPlugin)));
            pluginList.Clear();

            foreach (Type type in imp)
            {
                pluginList.Add((IPlugin)Activator.CreateInstance(type));
            }

            return pluginList;
        }

        public static List<string> GetPluginNames()
        {
            Type[] types = Assembly.GetExecutingAssembly().GetTypes();
            IEnumerable<Type> imp = types.Where(t => t.GetInterfaces().Contains(typeof(IPlugin)));

            return imp.Select(item => item.Name).ToList();
        }
    }
}
