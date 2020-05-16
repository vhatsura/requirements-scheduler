using System;

namespace RequirementsScheduler.BLL.Model.Conflicts
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
}
