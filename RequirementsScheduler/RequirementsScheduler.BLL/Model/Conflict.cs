namespace RequirementsScheduler.BLL.Model
{
    public class Conflict : BaseConflict<LaboriousDetail>, IChainNode
    {
        public ChainType Type => ChainType.Conflict;

        public void AddDetail(IChainNode node)
        {
            if (node.Type == ChainType.Conflict)
                DetailsDictionary.AddRange((node as Conflict).DetailsDictionary);
            else
            {
                var laboriousDetail = node as LaboriousDetail;
                DetailsDictionary.Add(laboriousDetail.Number, laboriousDetail);
            }
                    
        }
    }

    public class OnlineConflict : BaseConflict<Detail>, IOnlineChainNode
    {
        OnlineChainType IOnlineChainNode.Type => OnlineChainType.Conflict;
    }
}
