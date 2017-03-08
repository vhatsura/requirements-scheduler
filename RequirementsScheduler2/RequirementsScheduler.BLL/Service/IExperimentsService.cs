using System.Collections.Generic;
using RequirementsScheduler.BLL.Model;

namespace RequirementsScheduler.Core.Service
{
    public interface IExperimentsService
    {
        IEnumerable<Experiment> GetAll(string username);
        IEnumerable<Experiment> GetByStatus(ExperimentStatus status, string username);
        Experiment AddExperiment(Experiment value, string username);
    }
}
