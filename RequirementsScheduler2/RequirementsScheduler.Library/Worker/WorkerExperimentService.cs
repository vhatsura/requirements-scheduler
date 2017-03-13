using System;
using AutoMapper;
using RequirementsScheduler.BLL.Service;
using RequirementsScheduler.Core.Service;
using RequirementsScheduler.DAL.Repository;
using Experiment = RequirementsScheduler.DAL.Model.Experiment;

namespace RequirementsScheduler.Library.Worker
{
    public sealed class WorkerExperimentService : ExperimentsService, IWorkerExperimentService
    {
        public WorkerExperimentService(
            IMapper mapper,
            IUserService userService,
            IRepository<Experiment, Guid> repository) 
            : base(mapper, userService, repository)
        {
        }

        public void StartExperiment(Guid experimentId)
        {
            
        }

        public void StopExperiment(Guid experimentId)
        {
            
        }
    }
}
