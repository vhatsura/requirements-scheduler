using System.Collections.Generic;
using System.Linq;

namespace RequirementsScheduler.BLL.Model
{
    public class Chain : LinkedList<IChainNode>, IMachine
    {
        public Chain()
        {
            
        }

        public Chain(IEnumerable<IChainNode> collection)
            : base(collection)
        {
            
        }

        public bool IsOptimized => this.All(element => element.Type == ChainType.Detail);
    }
}
