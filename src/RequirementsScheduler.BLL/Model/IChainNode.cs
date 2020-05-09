using Newtonsoft.Json;

namespace RequirementsScheduler.BLL.Model
{
    public interface IChainNode
    {
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        ChainType Type { get; }
    }
}
