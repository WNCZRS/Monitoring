using System;
using System.Windows.Forms;
using System.Drawing;
using System.Threading;
using PluginsCollection;

namespace MonitoringAgent
{
    class TrayMenu
    {
        public static ContextMenu menu;

        public static NotifyIcon notificationIcon;
        public static MenuItem mnuPlugins = new MenuItem("Plugins");
        public static MenuItem line = new MenuItem("-");
        public static MenuItem mnuExit = new MenuItem("E&xit");
        public static MenuItem pluginSubMenu;

        public static void Notify()
        {
            Thread notifyThread = new Thread(
            delegate ()
            {
                SetMenu();

                notificationIcon = new NotifyIcon()
                {
                    Icon = new Icon("MonitoringAgent.ico"), //Properties.Resources.Services,
                    ContextMenu = menu,
                    Text = "MonitoringAgent",
                };
               
                notificationIcon.Visible = true;
                Application.Run();
            }
        );
            notifyThread.Start();

            //PluginOutputCollection plugOutput = new PluginOutputCollection();
            //output.CollectionList.Clear();

            //foreach (var plugin in AgentService._plugins.PluginList)
            //{
            //    plugOutput = plugin.Output();
            //    if (plugOutput != null)
            //    {
            //        output.CollectionList.Add(plugOutput);
            //        _log.Debug("Plugin loaded: " + plugin.ToString());
            //    }
            //}
        }
        static void mnuExit_Click(object sender, EventArgs e)
        {
            notificationIcon.Dispose();
            Application.Exit();
            Environment.Exit(0);
        }
        public static void BaloonTip(string text, int duration)
        {
            notificationIcon.BalloonTipText = text;
            notificationIcon.ShowBalloonTip(duration);
        }

        private static void SetMenu ()
        {
            menu = new ContextMenu();

            menu.MenuItems.Add(0, mnuPlugins);
            menu.MenuItems.Add(1, line);
            menu.MenuItems.Add(2, mnuExit);

            mnuExit.Click += new EventHandler(mnuExit_Click);
        }
        public static void pluginMenu(string plugOutput)
        {
            if (plugOutput != null)
            {
                pluginSubMenu = new MenuItem();
                pluginSubMenu.Text = plugOutput;

                if (pluginSubMenu == null)
                {
                    mnuPlugins.MenuItems.Add(pluginSubMenu);
                    pluginSubMenu.Click += new EventHandler(pluginSubMenu_Click);
                }

                else if (mnuPlugins.MenuItems.Count == 0)
                {
                    mnuPlugins.MenuItems.Add(pluginSubMenu);
                    pluginSubMenu.Click += new EventHandler(pluginSubMenu_Click);
                }
                else
                {
                    mnuPlugins.MenuItems.Add(pluginSubMenu);
                    pluginSubMenu.Click += new EventHandler(pluginSubMenu_Click);
                }
            }
        }
        static void pluginSubMenu_Click(object sender, EventArgs e)
        {
            MenuItem menuItem = (MenuItem)sender;

            MessageBox.Show("You clicked on " + menuItem.Text);
        }
    }
}
