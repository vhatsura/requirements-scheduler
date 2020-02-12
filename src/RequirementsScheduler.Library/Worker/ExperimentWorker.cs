using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Quartz;
using RequirementsScheduler.BLL.Model;
using RequirementsScheduler.BLL.Service;

namespace RequirementsScheduler.Library.Worker
{
    [DisallowConcurrentExecution]
    public sealed class ExperimentWorker : IJob
    {
        private readonly ILogger<ExperimentWorker> _logger;

        public ExperimentWorker(IExperimentsService service, IExperimentPipeline pipeline, ILogger<ExperimentWorker> logger)
        {
            _logger = logger;
            Service = service;
            Pipeline = pipeline;
        }

        private IExperimentsService Service { get; }
        private IExperimentPipeline Pipeline { get; }

        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                var experimentsForProcessing = Service.GetByStatus(ExperimentStatus.New, "worker");
                await Pipeline.Run(experimentsForProcessing);
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "Exception occurred during pipeline run.");
            }
        }
    }
}