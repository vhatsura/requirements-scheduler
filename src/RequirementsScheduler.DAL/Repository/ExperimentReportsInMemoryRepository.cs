using System.Threading;
using RequirementsScheduler.DAL.Model;

namespace RequirementsScheduler.DAL.Repository
{
    public class ExperimentReportsInMemoryRepository : InMemoryRepository<ExperimentResult, int>
    {
        private int _id;

        protected override int NextId => Interlocked.Increment(ref _id);
    }
}
