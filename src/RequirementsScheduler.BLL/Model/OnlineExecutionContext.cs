using System;

namespace RequirementsScheduler.BLL.Model
{
    public class OnlineExecutionContext
    {
        public bool IsResolvedOnCheck3InOnline { get; set; }

        public int UnresolvedConflictAmount { get; set; }

        public int ResolvedConflictAmount { get; set; }

        public TimeSpan ExecutionTime { get; set; }

        public double TimeFromMachinesStart { get; set; }

        public double Time1 { get; set; }

        public double Time2 { get; set; }
    }
}
