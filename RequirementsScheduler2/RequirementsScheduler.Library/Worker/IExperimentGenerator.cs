using RequirementsScheduler.Core.Model;

namespace RequirementsScheduler.Core.Worker
{
    public interface IExperimentGenerator
    {
        ExperimentInfo GenerateDataForTest(Experiment experiment);
    }
}
