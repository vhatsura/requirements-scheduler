using Newtonsoft.Json;

namespace RequirementsScheduler.BLL.Model
{
    public interface IOnlineChainNode : IPGenerator
    {
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        OnlineChainType Type { get; }
    }
}
