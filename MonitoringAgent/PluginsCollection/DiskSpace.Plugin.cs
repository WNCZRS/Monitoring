using System;
using System.IO;

namespace PluginsCollection
{
    public class DiskSpace : IPlugin
    {
        PluginOutputCollection _pluginOutputs;

        public string Name
        {
            get
            {
                return "Disk free space";
            }
        }

        public DiskSpace()
        {
            _pluginOutputs = new PluginOutputCollection(Name);
        }

        public PluginOutputCollection Output()
        {
            DriveInfo[] allDrives = DriveInfo.GetDrives();
            string freeSpace = string.Empty;
            _pluginOutputs.PluginOutputList.Clear();

            foreach (DriveInfo drive in allDrives)
            {
                if (drive.DriveType == DriveType.Fixed)
                {
                    freeSpace = Math.Round((drive.AvailableFreeSpace / Math.Pow(1024, 3)), 2).ToString() + " GB";
                    _pluginOutputs.NewPluginOutput(drive.Name, freeSpace);
                }
            }
            return _pluginOutputs;
        }
    }
}
