using System.Collections.Generic;
using System.Threading.Tasks;
using RequirementsScheduler.DAL.Repository;
using RequirementsScheduler.Core.Model;

namespace RequirementsScheduler.Core.Worker
{
    public sealed class ExperimentPipeline
    {
        private readonly IRepository<Experiment> Repository = new ExperimentsRepository();

        public async Task Run(IEnumerable<Experiment> experiments)
        {
            foreach (var experiment in experiments)
            {
                experiment.Status = ExperimentStatus.InProgress;
                Repository.Update(experiment);

                await RunTest(experiment);

                experiment.Status = ExperimentStatus.Completed;
                Repository.Update(experiment);
            }
        }

        private async Task RunTest(Experiment experiment)
        {
            for (var i = 0; i < experiment.TestsAmount; i++)
            {
                await GenerateDataForTest(experiment);
            }
        }

        private Task GenerateDataForTest(Experiment experiment)
        {
            var firstRequirementsAmount = experiment.RequirementsAmount * experiment.N1;
            var secondRequirementsAmount = experiment.RequirementsAmount * experiment.N2;
            var firstSecondRequirementsAmount = experiment.RequirementsAmount * experiment.N12;
            var secondFirstRequirementsAmount = experiment.RequirementsAmount * experiment.N21;

            for(int i = 0; i < firstRequirementsAmount; i++) 
            {

            }

            for(int i = 0; i < secondRequirementsAmount; i++) 
            {

            }

            for(int i = 0; i < firstSecondRequirementsAmount; i++) 
            {

            }

            for(int i = 0; i < secondFirstRequirementsAmount; i++) 
            {

            }

            return Task.FromResult(0);
        }
    }
}
