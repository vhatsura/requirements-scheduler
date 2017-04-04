using System;
using System.Collections.Generic;
using RequirementsScheduler.BLL.Model;

namespace RequirementsScheduler.BLL.Service
{
    public interface IExperimentsService
    {
        Experiment Get(Guid experimentId, string userName);
        IEnumerable<Experiment> GetAll(string username);
        IEnumerable<Experiment> GetByStatus(ExperimentStatus status, string username);
        Experiment AddExperiment(Experiment value, string username);
    }
}
