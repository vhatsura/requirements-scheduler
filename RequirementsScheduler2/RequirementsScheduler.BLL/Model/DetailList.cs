using System.Collections.Generic;

namespace RequirementsScheduler.BLL.Model
{
    public class DetailList : List<Detail>, IMachine
    {
        public bool IsOptimized => true;
    }
}
