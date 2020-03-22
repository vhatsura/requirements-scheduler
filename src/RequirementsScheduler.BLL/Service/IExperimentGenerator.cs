using RequirementsScheduler.BLL.Model;

namespace RequirementsScheduler.BLL.Service
{
    public interface IExperimentGenerator
    {
        ExperimentInfo GenerateDataForTest(Experiment experiment, int testNumber);
        void GenerateP(IOnlineChainNode node);
    }
}