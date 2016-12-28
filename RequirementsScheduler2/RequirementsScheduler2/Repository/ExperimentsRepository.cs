using RequirementsScheduler2.Models;

namespace RequirementsScheduler2.Repository
{
    public class ExperimentsRepository : Repository<Experiment>
    {
        protected override void BeforeAdd(Experiment value)
        {
            value.Status = ExperimentStatus.New;
        }
    }
}
