namespace RequirementsScheduler.Core.Model
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
