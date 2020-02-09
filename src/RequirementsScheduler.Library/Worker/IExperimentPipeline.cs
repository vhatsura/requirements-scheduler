using System.Collections.Generic;
using System.Threading.Tasks;
using RequirementsScheduler.BLL.Model;

namespace RequirementsScheduler.Library.Worker
{
    public interface IExperimentPipeline
    {
        Task Run(IEnumerable<Experiment> experiments);
    }
}