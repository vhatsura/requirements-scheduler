using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace RequirementsScheduler.BLL.Model
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum ChainType
    {
        Detail,
        Conflict
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum OnlineChainType
    {
        Detail,
        Conflict,
        Downtime
    }
}
