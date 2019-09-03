using RequirementsScheduler.BLL.Model;

namespace RequirementsScheduler.Library.Worker
{
    public interface IExperimentGenerator
    {
        ExperimentInfo GenerateDataForTest(Experiment experiment, int testNumber);
    }
}
