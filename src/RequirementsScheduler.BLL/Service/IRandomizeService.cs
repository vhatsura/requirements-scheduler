using RequirementsScheduler.DAL.Model;

namespace RequirementsScheduler.BLL.Service
{
    public interface IRandomizeService
    {
        double GetRandomDouble(double min, double max, Distribution distribution);

        double GetRandomDouble(int min, int max, Distribution distribution = Distribution.Uniform);
    }
}
