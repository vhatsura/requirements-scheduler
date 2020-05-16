using RequirementsScheduler.BLL.Service;

namespace RequirementsScheduler.BLL.Model
{
    public interface IPGenerator
    {
        void GenerateP(IRandomizeService randomizeService);
    }
}
