using System.Collections.Generic;
using System.Threading.Tasks;
using RequirementsScheduler2.Models;
using RequirementsScheduler2.Repository;

namespace RequirementsScheduler2.Worker
{
    public class ExperimentPipeline
    {
        private readonly ExperimentsRepository Repository = new ExperimentsRepository();

        public async Task Run(IEnumerable<Experiment> experiments)
        {
            foreach (var experiment in experiments)
            {
                experiment.Status = ExperimentStatus.InProgress;
                await RunTest(experiment);
                experiment.Status = ExperimentStatus.Completed;
            }
        }

        private async Task RunTest(Experiment experiment)
        {
            for (var i = 0; i < experiment.TestsAmount; i++)
            {
                await GenerateDataForTest();
            }
        }

        private Task GenerateDataForTest()
        {
            return Task.FromResult(0);
        }
    }
}
