using System.Collections.Generic;

namespace RequirementsScheduler.BLL.Model
{
    public class LaboriousDetailList : List<LaboriousDetail>, IMachine
    {
        public bool IsOptimized { get; set; }
    }
}
