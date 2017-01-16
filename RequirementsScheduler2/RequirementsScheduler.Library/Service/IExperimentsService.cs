using System.Collections.Generic;
using RequirementsScheduler.Core.Model;

namespace RequirementsScheduler.Core.Service
{
    public interface IExperimentsService
    {
        IEnumerable<Experiment> GetAll();
        void AddExperiment(Experiment value);
    }
}
