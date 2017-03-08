using System;

namespace RequirementsScheduler.DAL.Repository
{
    public class ExperimentReportsInMemoryRepository : InMemoryRepository<DAL.Model.ExperimentResult, Guid>
    {
        protected override Guid NextId => Guid.NewGuid();
    }
}
