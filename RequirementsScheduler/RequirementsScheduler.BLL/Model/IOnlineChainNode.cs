using Newtonsoft.Json;

namespace RequirementsScheduler.BLL.Model
{
    public interface IOnlineChainNode
    {
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        OnlineChainType Type { get; }
    }

}
