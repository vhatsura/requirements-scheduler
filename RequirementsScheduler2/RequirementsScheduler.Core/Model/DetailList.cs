using System.Collections.Generic;

namespace RequirementsScheduler.Core.Model
{
    public class DetailList : List<Detail>, IMachine
    {
        public bool IsOptimized => true;
    }
}
