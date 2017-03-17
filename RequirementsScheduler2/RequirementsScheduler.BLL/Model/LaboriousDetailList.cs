using System.Collections.ObjectModel;

namespace RequirementsScheduler.BLL.Model
{
    public class LaboriousDetailList : Collection<LaboriousDetail>, IMachine
    {
        public bool IsOptimized { get; set; }
    }
}
