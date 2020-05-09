using System;
using Microsoft.AspNetCore.Builder;
using Quartz.Spi;

namespace RequirementsScheduler.WebApiHost.Extensions
{
    public static class QuartzExtensions
    {
        internal static void UseQuartz(this IApplicationBuilder app, Action<Quartz.Quartz> configuration)
        {
            // Job Factory through IOC container
            var jobFactory = (IJobFactory) app.ApplicationServices.GetService(typeof(IJobFactory));
            // Set job factory
            Quartz.Quartz.Instance.UseJobFactory(jobFactory);

            // Run configuration
            configuration.Invoke(Quartz.Quartz.Instance);
            // Run Quartz
            Quartz.Quartz.Start();
        }
    }
}
