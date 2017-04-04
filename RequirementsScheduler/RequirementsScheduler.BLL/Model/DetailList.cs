using System.Collections.ObjectModel;

namespace RequirementsScheduler.BLL.Model
{
    public class DetailList : Collection<Detail>, IMachine
    {
        public bool IsOptimized => true;
    }
}
