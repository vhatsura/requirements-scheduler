using System.Collections.Generic;
using RequirementsScheduler.Core.Model;
using RequirementsScheduler.Core.Service;
using RequirementsScheduler.DAL.Repository;

namespace RequirementsScheduler.Core
{
    public class ExperimentsService : IExperimentsService
    {
        private readonly IRepository<Experiment> ExperimentsRepository = new ExperimentsRepository();

        public IEnumerable<Experiment> GetAll()
        {
            return ExperimentsRepository.Get();
        }

        public void AddExperiment(Experiment value)
        {
            ExperimentsRepository.Add(value);
        }
    }
}
