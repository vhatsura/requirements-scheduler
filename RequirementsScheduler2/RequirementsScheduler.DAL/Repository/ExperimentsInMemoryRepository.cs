using System;
using Experiment = RequirementsScheduler.DAL.Model.Experiment;

namespace RequirementsScheduler.DAL.Repository
{
    public sealed class ExperimentsInMemoryRepository : InMemoryRepository<Experiment, Guid>
    {
        protected override Guid NextId => Guid.NewGuid();
    }
}
