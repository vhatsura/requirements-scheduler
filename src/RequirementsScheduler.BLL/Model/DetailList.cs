using System.Collections.ObjectModel;
using System.Diagnostics;

namespace RequirementsScheduler.BLL.Model
{
    [DebuggerDisplay("IsOptimized: {" + nameof(IsOptimized) + "}, Count: {this." + nameof(Count) + "}")]
    public class DetailList : Collection<Detail>, IMachine
    {
        public bool IsOptimized => true;
    }
}
