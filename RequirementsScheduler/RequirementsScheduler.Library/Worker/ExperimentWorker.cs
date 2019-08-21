using System;
using System.Threading.Tasks;
using Quartz;
using RequirementsScheduler.BLL.Model;
using RequirementsScheduler.BLL.Service;

namespace RequirementsScheduler.Library.Worker
{
    [DisallowConcurrentExecution]
    public sealed class ExperimentWorker : IJob
    {
        private IExperimentsService Service { get; }
        private IExperimentPipeline Pipeline { get; }

        public ExperimentWorker(IExperimentsService service, IExperimentPipeline pipeline)
        {
            Service = service;
            Pipeline = pipeline;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                var experimentsForProcessing = Service.GetByStatus(ExperimentStatus.New, "worker");
                await Pipeline.Run(experimentsForProcessing);
            }
            catch (Exception ex)
            {
            }
            
        }
    }
}
