namespace MonitoringAgent
{
    class Program
    {
        public static void Main(string[] args)
        {
            // Configure service using Topshelf dll
            ConfigureService.Configure();
        }
    }
}
