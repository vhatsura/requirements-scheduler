namespace RequirementsScheduler.BLL.Model
{
    public class Conflict : BaseConflict<LaboriousDetail>, IChainNode
    {
        public ChainType Type => ChainType.Conflict;
    }

    public class OnlineConflict : BaseConflict<Detail>, IOnlineChainNode
    {
        OnlineChainType IOnlineChainNode.Type => OnlineChainType.Conflict;
    }
}
