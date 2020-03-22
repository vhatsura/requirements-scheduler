using System.Collections.Generic;
using RequirementsScheduler.BLL.Service;

namespace RequirementsScheduler.BLL.Model
{
    public sealed class OnlineChain : LinkedList<IOnlineChainNode>, IPGenerator
    {
        public void GenerateP(IRandomizeService randomizeService)
        {
            foreach (var node in this) node.GenerateP(randomizeService);
        }
    }
}