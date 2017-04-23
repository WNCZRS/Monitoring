using System;
using System.Collections.Generic;
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
            List<SimplePluginOutput> listSPO;
            string freeSpace = string.Empty;
            string freeSpacePercent = string.Empty;
            string totalSpace = string.Empty;
            DriveInfo[] allDrives = DriveInfo.GetDrives();

            _pluginOutputs.PluginOutputList.Clear();

            List<SimplePluginOutput> headListSPO = new List<SimplePluginOutput>();
            headListSPO.Add(new SimplePluginOutput("Free Space (GB)", false));
            headListSPO.Add(new SimplePluginOutput("Free Space (%)", false));
            headListSPO.Add(new SimplePluginOutput("Total Space (GB)", false));
            _pluginOutputs.PluginOutputList.Add(new PluginOutput("", headListSPO));
            
            foreach (DriveInfo drive in allDrives)
            {
                if (drive.DriveType == DriveType.Fixed)
                {
                    listSPO = new List<SimplePluginOutput>();
                    freeSpace = Math.Round((drive.AvailableFreeSpace / Math.Pow(1024, 3)), 2).ToString() + " GB";
                    listSPO.Add(new SimplePluginOutput(freeSpace, false));

                    freeSpacePercent = Math.Round((drive.AvailableFreeSpace / Math.Pow(1024, 3)) / (drive.TotalSize / Math.Pow(1024, 3)) * 100, 2).ToString() + " %";
                    listSPO.Add(new SimplePluginOutput(freeSpacePercent, false));

                    totalSpace = Math.Round((drive.TotalSize / Math.Pow(1024, 3)), 2).ToString() + " GB";
                    listSPO.Add(new SimplePluginOutput(totalSpace, false));
                    _pluginOutputs.PluginOutputList.Add(new PluginOutput(drive.Name, listSPO));
                }
            }
            return _pluginOutputs;
        }
    }
}
