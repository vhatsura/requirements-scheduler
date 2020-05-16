using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Newtonsoft.Json;
using RequirementsScheduler.BLL.Service;
using RequirementsScheduler.DAL.Model;

namespace RequirementsScheduler.BLL.Model.Conflicts
{
    [DebuggerDisplay("{ToString()}")]
    public class OnlineConflict : BaseConflict<Detail>, IOnlineChainNode
    {
        [JsonConstructor]
        public OnlineConflict(Dictionary<int, Detail> details)
        {
            Details.AddRange(details);
        }

        public OnlineConflict(IEnumerable<KeyValuePair<int, Detail>> details)
        {
            Details.AddRange(details);
        }

        OnlineChainType IOnlineChainNode.Type => OnlineChainType.Conflict;

        // public ProcessingTime Time
        // {
        //     get
        //     {
        //         var a = Details.Values.Sum(x => x.Time.A);
        //         var b = Details.Values.Sum(x => x.Time.B);
        //         var p = Details.Values.Sum(x => x.Time.P);
        //
        //         return new ProcessingTime(a, b, p, Distribution.None);
        //     }
        // }

        public void GenerateP(IRandomizeService randomizeService)
        {
            foreach (var detail in Details.Values) detail.GenerateP(randomizeService);
        }

        public override string ToString() =>
            $"{Details.Count} details. #({Details.Values.FirstOrDefault()?.Number ?? 0})";
    }
}
