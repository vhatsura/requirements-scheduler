using System;
using Quartz;
using Quartz.Spi;

namespace RequirementsScheduler.Library.Worker
{
    public sealed class WorkerJobFactory : IJobFactory
    {
        private IServiceProvider Container { get; }

        public WorkerJobFactory(IServiceProvider container)
        {
            Container = container;
        }

        public IJob NewJob(TriggerFiredBundle bundle, IScheduler scheduler)
        {
            return Container.GetService(bundle.JobDetail.JobType) as IJob;
        }

        public void ReturnJob(IJob job)
        {
            if (job is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }
}
