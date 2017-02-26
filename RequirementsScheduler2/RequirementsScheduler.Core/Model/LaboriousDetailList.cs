using System.Collections.Generic;

namespace RequirementsScheduler.Core.Model
{
    public class LaboriousDetailList : List<LaboriousDetail>, IMachine
    {
        public bool IsOptimized { get; set; }
    }
}
