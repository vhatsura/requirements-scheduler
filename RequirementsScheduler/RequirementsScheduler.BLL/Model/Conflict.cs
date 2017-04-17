namespace RequirementsScheduler.BLL.Model
{
    public class Conflict : BaseConflict<LaboriousDetail>, IChainNode, IOnlineChainNode
    {
        public ChainType Type => ChainType.Conflict;

        OnlineChainType IOnlineChainNode.Type => OnlineChainType.Conflict;
    }
}
