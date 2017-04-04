using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace RequirementsScheduler.BLL.Model
{
    public interface IChainNode
    {
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        ChainType Type { get; }
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum ChainType
    {
        Detail,
        Conflict
    }
}
