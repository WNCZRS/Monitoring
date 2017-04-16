using System;
using System.Collections.Generic;
using System.IO;
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
        public bool IsCritical { get; set; }

        public SimplePluginOutput(object value, bool isCritical)
        {
            Value = value;
            IsCritical = isCritical;
        }
    }

    public class PluginOutputCollection
    {
        public string PluginName { get; set; }
        public List<PluginOutput> PluginOutputList { get; }

        public PluginOutputCollection(string name = "")
        {
            PluginName = name;
            PluginOutputList = new List<PluginOutput>();
        }

        public void NewPluginOutput(string name, List<SimplePluginOutput> listOfSimplePluginOutput)
        {
            PluginOutputList.Add(new PluginOutput(name, listOfSimplePluginOutput));
        }
    }

    public class ClientOutput
    {
        public List<PluginOutputCollection> CollectionList { get; set; }
        public string ID { get; }
        public string PCName { get; set; }
        public string Customer { get; set; }
        public bool InitPost { get; set; }
        public DateTime LastUpdate { get; set; }

        public ClientOutput(string pcName, string id, string customer)
        {
            PCName = pcName;
            ID = id;
            Customer = customer;
            CollectionList = new List<PluginOutputCollection>();
            InitPost = false;
            LastUpdate = DateTime.MinValue;
        }

        public PluginOutputCollection GetPluginOutputByName(string pluginName)
        {
            return CollectionList.Find(item => item.PluginName == pluginName);
        }
    }

    public class PluginLoader
    {
        public List<IPlugin> PluginList { get; set; }

        public PluginLoader()
        {
            PluginList = new List<IPlugin>();
        }

        public void Load()
        {
            LoadSystemPlugins();
            LoadDllPlugins();
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

        private void LoadDllPlugins()
        {
            string path = "Plugins";

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
                            if (!PluginList.Any(item => item.Name == plugin.Name))
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
