using System.Collections.ObjectModel;
using System.Diagnostics;

namespace RequirementsScheduler.BLL.Model
{
    [DebuggerDisplay("IsOptimized: {" + nameof(IsOptimized) + "}, Count: {this." + nameof(Count) + "}")]
    public class LaboriousDetailList : Collection<LaboriousDetail>, IMachine
    {
        public bool IsOptimized { get; set; }
    }
}
