using System.Collections.Generic;

namespace RequirementsScheduler.BLL.Model
{
    public sealed class OnlineChain : LinkedList<IOnlineChainNode>
    {
        public void GenerateP()
        {
            foreach (var node in this)
            {
                node.GenerateP();
            }
        }
    }
}
