using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using RequirementsScheduler.BLL.Model;

namespace RequirementsScheduler.BLL.Service
{
    public interface IExperimentsService
    {
        Experiment Get(Guid experimentId, string userName, params Expression<Func<Experiment, object>>[] membersToLoad);
        IEnumerable<Experiment> GetAll(string username);
        IEnumerable<Experiment> GetByStatus(ExperimentStatus status, string username);
        Experiment AddExperiment(Experiment value, string username);
    }
}
