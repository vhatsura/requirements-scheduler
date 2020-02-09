using System;
using RequirementsScheduler.DAL.Model;

namespace RequirementsScheduler.DAL.Repository
{
    public sealed class ExperimentsInMemoryRepository : InMemoryRepository<Experiment, Guid>
    {
        protected override Guid NextId => Guid.NewGuid();
    }
}