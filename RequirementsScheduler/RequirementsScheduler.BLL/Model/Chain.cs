using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace RequirementsScheduler.BLL.Model
{
    [DebuggerDisplay("IsOptimized: {" + nameof(IsOptimized) + "}, Count: {this." + nameof(Count) + "}")]
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
