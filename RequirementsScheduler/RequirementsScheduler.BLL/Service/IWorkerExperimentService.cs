using System;

namespace RequirementsScheduler.BLL.Service
{
    public interface IWorkerExperimentService : IExperimentsService
    {
        void StartExperiment(Guid experimentId);
        void StopExperiment(Guid experimentId);
    }
}