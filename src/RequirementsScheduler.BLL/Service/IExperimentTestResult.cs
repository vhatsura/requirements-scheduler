using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RequirementsScheduler.BLL.Model;

namespace RequirementsScheduler.BLL.Service
{
    public interface IExperimentTestResultService
    {
        Task SaveExperimentTestResult(Guid experimentId, ExperimentInfo experimentInfo);
        Task<ExperimentInfo> GetExperimentTestResult(Guid experimentId, int testNumber);

        Task SaveAggregatedResult(Guid experimentId, IDictionary<int, ResultInfo> aggregatedResult);
        Task<IDictionary<int, ResultInfo>> GetAggregatedResult(Guid experimentId);
    }
}
