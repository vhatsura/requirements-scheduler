using Quartz;
using RequirementsScheduler.Core.Model;
using RequirementsScheduler.DAL.Repository;
using System.Threading.Tasks;

namespace RequirementsScheduler.Core.Worker
{
    public sealed class ExperimentWorker : IJob
    {
        private readonly IRepository<Experiment> Repository = new ExperimentsRepository();

        public async Task Execute(IJobExecutionContext context)
        {
            var experimentsForProcessing = Repository.Get(experiment => experiment.Status == ExperimentStatus.New);
            var pipeline = new ExperimentPipeline(new ExperimentGenerator());
            await pipeline.Run(experimentsForProcessing);
        }
    }
}
