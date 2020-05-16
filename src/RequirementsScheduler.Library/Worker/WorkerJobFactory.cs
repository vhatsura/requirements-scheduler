using System;
using Quartz;
using Quartz.Spi;

namespace RequirementsScheduler.Library.Worker
{
    public sealed class WorkerJobFactory : IJobFactory
    {
        public WorkerJobFactory(IServiceProvider container)
        {
            Container = container;
        }

        private IServiceProvider Container { get; }

        public IJob NewJob(TriggerFiredBundle bundle, IScheduler scheduler) =>
            Container.GetService(bundle.JobDetail.JobType) as IJob;

        public void ReturnJob(IJob job)
        {
            if (job is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }
}
