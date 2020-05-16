using System.Collections.Generic;
using RequirementsScheduler.BLL.Model;

namespace RequirementsScheduler.Library.Worker
{
    public interface IOnlineExecutor
    {
        OnlineExecutionContext Execute(OnlineChain onlineChainOnFirst,
            OnlineChain onlineChainOnSecond, HashSet<int> processedDetailsOnFirst,
            HashSet<int> processedDetailsOnSecond);
    }
}
