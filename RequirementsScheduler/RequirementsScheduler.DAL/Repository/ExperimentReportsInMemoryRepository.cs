using System.Threading;

namespace RequirementsScheduler.DAL.Repository
{
    public class ExperimentReportsInMemoryRepository : InMemoryRepository<DAL.Model.ExperimentResult, int>
    {
        private int _id;

        protected override int NextId => Interlocked.Increment(ref _id);
    }
}
