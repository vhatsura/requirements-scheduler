using System;
using RequirementsScheduler.Core.Service;

namespace RequirementsScheduler.BLL.Service
{
    public interface IWorkerExperimentService : IExperimentsService
    {
        void StartExperiment(Guid experimentId);
        void StopExperiment(Guid experimentId);
    }
}
