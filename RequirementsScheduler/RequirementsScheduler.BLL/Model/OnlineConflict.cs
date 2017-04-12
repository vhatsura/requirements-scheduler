namespace RequirementsScheduler.BLL.Model
{
    public class OnlineConflict : BaseConflict<Detail>, IOnlineChainNode
    {
        public OnlineChainType Type => OnlineChainType.Conflict;
    }
}
