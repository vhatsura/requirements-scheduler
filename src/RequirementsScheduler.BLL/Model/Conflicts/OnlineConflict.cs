using System.Collections.Generic;
using Newtonsoft.Json;
using RequirementsScheduler.BLL.Service;

namespace RequirementsScheduler.BLL.Model.Conflicts
{
    public class OnlineConflict : BaseConflict<Detail>, IOnlineChainNode
    {
        [JsonConstructor]
        public OnlineConflict(Dictionary<int, Detail> details)
        {
            Details.AddRange(details);
        }

        public OnlineConflict(IEnumerable<KeyValuePair<int, Detail>> details)
        {
            Details.AddRange(details);
        }

        OnlineChainType IOnlineChainNode.Type => OnlineChainType.Conflict;

        public void GenerateP(IRandomizeService randomizeService)
        {
            foreach (var detail in Details.Values) detail.GenerateP(randomizeService);
        }
    }
}
