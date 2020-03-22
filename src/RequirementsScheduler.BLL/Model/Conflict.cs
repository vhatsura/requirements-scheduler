using System;
using System.Collections.Generic;

namespace RequirementsScheduler.BLL.Model
{
    public class Conflict : BaseConflict<LaboriousDetail>, IChainNode
    {
        public ChainType Type => ChainType.Conflict;

        public void AddDetails(IChainNode node)
        {
            switch (node.Type)
            {
                case ChainType.Conflict when node is Conflict conflict:
                    Details.AddRange(conflict.Details);
                    break;
                case ChainType.Detail when node is LaboriousDetail detail:
                    Details.Add(detail.Number, detail);
                    break;
                default: throw new InvalidOperationException();
            }
        }
    }

    public class OnlineConflict : BaseConflict<Detail>, IOnlineChainNode
    {
        public OnlineConflict(IEnumerable<KeyValuePair<int, Detail>> details)
        {
            Details.AddRange(details);
        }

        OnlineChainType IOnlineChainNode.Type => OnlineChainType.Conflict;

        public void GenerateP()
        {
            foreach (var detail in Details.Values) detail.GenerateP();
        }
    }
}