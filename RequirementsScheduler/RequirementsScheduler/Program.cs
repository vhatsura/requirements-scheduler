using System.Diagnostics;
using System.IO;
using Microsoft.AspNetCore.Hosting;

namespace RequirementsScheduler
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var pathToExe = Directory.GetCurrentDirectory();
            var pathToContentRoot = Path.GetDirectoryName(pathToExe)?
                .Replace(@"\bin\Debug\netcoreapp2.0", "")
                .Replace(@"\bin\Release\netcoreapp2.0", "")
                .Replace(@"\bin\Debug", "")
                .Replace(@"\bin\Release", "");

            var host = new WebHostBuilder()
                .UseKestrel()
                .CaptureStartupErrors(true)
                .UseSetting("detailedErrors", "true")
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseIISIntegration()
                .UseStartup<Startup>()
                .Build();

            host.Run();
        }
    }
}
