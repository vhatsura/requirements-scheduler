using RequirementsScheduler.BLL.Model;

namespace RequirementsScheduler.Core.Worker
{
    public interface IExperimentGenerator
    {
        ExperimentInfo GenerateDataForTest(Experiment experiment);
    }
}
