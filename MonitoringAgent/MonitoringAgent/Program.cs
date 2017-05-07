using Topshelf;
using System;
using System.Runtime.InteropServices;

namespace MonitoringAgent
{
    class Program
    {
        // Get Console Windows controls
        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        // Windows visibility controls
        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        // Methods to HIDE or SHOW application window
        const int SW_HIDE = 0;
        const int SW_SHOW = 5;

        public static int Main(string[] args)
        {
            var handle = GetConsoleWindow();

            // Hide console window
            //ShowWindow(handle, SW_HIDE);

            // Show console window
            ShowWindow(handle, SW_SHOW);

            // Run application as a Windows service
            return (int)HostFactory.Run(x =>
            {
                x.Service<AgentService>(s =>
                {
                    s.ConstructUsing(() => new AgentService());
                    s.WhenStarted(service => service.Start());
                    s.WhenStopped(service => service.Stop());
                });
            });
        }
    }
    public class MonitoringAgentService
    {
        public void Start()
        {
            ConfigureService.Configure();
        }
        public void Stop()
        {
      
        }
    }
}
