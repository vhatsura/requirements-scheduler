using System.Threading.Tasks;
using Elastic.CommonSchema.Serilog;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Debugging;
using Serilog.Events;

namespace RequirementsScheduler.WebApiHost
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var hostBuilder = CreateHostBuilder(args);
            var host = hostBuilder.Build();

            var logger = host.Services.GetRequiredService<ILogger<Program>>();

            SelfLog.Enable(msg => logger.LogError(msg));

            await host.RunAsync();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); })
                .UseSerilog((hostBuilderContext, loggerConfiguration) =>
                {
                    var ecsTextFormatterConfiguration = new EcsTextFormatterConfiguration();
                    ((IEcsTextFormatterConfiguration) ecsTextFormatterConfiguration).MapCustom = (b, e) => b;

                    loggerConfiguration
                        .ReadFrom.Configuration(hostBuilderContext.Configuration)
                        .WriteTo.Elasticsearch("http://localhost:9200",
                            customFormatter: new EcsTextFormatter(ecsTextFormatterConfiguration), connectionTimeout: 20)
                        .WriteTo.Logger(lc => lc.Filter.ByExcluding(x =>
                        {
                            if (!x.TryGetScalarPropertyValue("SourceContext", out var value))
                            {
                                return false;
                            }

                            return value.ToString() == "\"RequirementsScheduler.Library.Worker.ExperimentPipeline\"" &&
                                   x.Level != LogEventLevel.Error && x.Level != LogEventLevel.Fatal;
                        }).WriteTo.Console())
                        .Enrich.FromLogContext();
                });
    }
}
