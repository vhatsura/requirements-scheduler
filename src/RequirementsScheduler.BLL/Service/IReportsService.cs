using System.Collections.Generic;
using RequirementsScheduler.BLL.Model;

namespace RequirementsScheduler.BLL.Service
{
    public interface IReportsService
    {
        IEnumerable<ExperimentReport> GetAll();
        void Save(ExperimentReport report);
    }
}