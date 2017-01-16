using RequirementsScheduler.Core.Model;

namespace RequirementsScheduler.DAL.Repository
{
    public sealed class ExperimentsRepository : Repository<Experiment>
    {
        protected override void BeforeAdd(Experiment value)
        {
            value.Status = ExperimentStatus.New;
        }
    }
}
