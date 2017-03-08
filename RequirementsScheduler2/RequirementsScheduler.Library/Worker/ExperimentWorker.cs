using Quartz;
using System.Threading.Tasks;
using RequirementsScheduler.BLL.Model;
using RequirementsScheduler.Core.Service;

namespace RequirementsScheduler.Core.Worker
{
    public sealed class ExperimentWorker : IJob
    {
        private IExperimentsService Service { get; }
        private ExperimentPipeline Pipeline { get; }

        public ExperimentWorker(IExperimentsService service, ExperimentPipeline pipeline)
        {
            Service = service;
            Pipeline = pipeline;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            var experimentsForProcessing = Service.GetByStatus(ExperimentStatus.New, "worker");
            await Pipeline.Run(experimentsForProcessing);
        }
    }
}
