using System;
using AutoMapper;
using Microsoft.Extensions.Logging;
using RequirementsScheduler.BLL.Model;
using RequirementsScheduler.BLL.Service;
using RequirementsScheduler.Core.Service;
using RequirementsScheduler.DAL.Repository;
using Experiment = RequirementsScheduler.DAL.Model.Experiment;

namespace RequirementsScheduler.Library.Worker
{
    public sealed class WorkerExperimentService : ExperimentsService, IWorkerExperimentService
    {
        private ILogger Logger { get; }

        public WorkerExperimentService(
            IMapper mapper,
            IUserService userService,
            IRepository<Experiment, Guid> repository,
            ILogger logger) 
            : base(mapper, userService, repository)
        {
            Logger = logger;
        }

        public void StartExperiment(Guid experimentId)
        {
            try
            {
                var experiment = Repository.Get(experimentId);
                experiment.Status = (int) ExperimentStatus.InProgress;
                Repository.Update(experimentId, experiment);
            }
            catch (Exception ex)
            {
                Logger.LogCritical(new EventId(), ex, "Error during start experiment");
            }
            
        }

        public void StopExperiment(Guid experimentId)
        {
            var experiment = Repository.Get(experimentId);
            experiment.Status = (int)ExperimentStatus.Completed;
            Repository.Update(experimentId, experiment);
        }
    }
}
