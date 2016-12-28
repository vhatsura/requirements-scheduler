using System.Threading.Tasks;
using Quartz;
using RequirementsScheduler2.Models;
using RequirementsScheduler2.Repository;

namespace RequirementsScheduler2.Worker
{
    public class ExperimentWorker : IJob
    {
        private readonly ExperimentsRepository Repository = new ExperimentsRepository();

        public async Task Execute(IJobExecutionContext context)
        {
            var experimentsForProcessing = Repository.Get(experiment => experiment.Status == ExperimentStatus.New);
            var pipeline = new ExperimentPipeline();
            await pipeline.Run(experimentsForProcessing);
        }
    }
}
