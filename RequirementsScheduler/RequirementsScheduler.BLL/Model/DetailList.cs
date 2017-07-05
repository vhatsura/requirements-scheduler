using System.Collections.ObjectModel;

namespace RequirementsScheduler.BLL.Model
{
    public class DetailList : Collection<Detail>, IMachine
    {
        public static DetailList Empty => new DetailList();
        public bool IsOptimized => true;
    }
}
