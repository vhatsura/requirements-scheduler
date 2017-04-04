using System;
using System.Threading.Tasks;
using RequirementsScheduler.BLL.Model;

namespace RequirementsScheduler.BLL.Service
{
    public interface IExperimentTestResultService
    {
        Task SaveExperimentTestResult(Guid experimentId, ExperimentInfo experimentInfo);
        Task<ExperimentInfo> GetExperimentTestResult(Guid experimentId, int testNumber);
    }
}
