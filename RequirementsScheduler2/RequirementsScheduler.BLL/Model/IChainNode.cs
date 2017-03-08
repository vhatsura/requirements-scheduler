namespace RequirementsScheduler.BLL.Model
{
    public interface IChainNode
    {
        ChainType Type { get; }
    }

    public enum ChainType
    {
        Detail,
        Conflict
    }
}
