using System.Diagnostics;
using Newtonsoft.Json;

namespace RequirementsScheduler.BLL.Model
{
    [DebuggerDisplay("Result: {Result.Type}")]
    public class ExperimentInfo : IMachine
    {
        public int TestNumber { get; set; }

        public bool IsOptimized => 
            J1.IsOptimized && 
            J2.IsOptimized && 
            (J12.IsOptimized || J12Chain != null && J12Chain.IsOptimized) && 
            (J21.IsOptimized || J21Chain != null && J21Chain.IsOptimized);

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public ResultInfo Result { get; } = new ResultInfo();
        
        public DetailList J1 { get; } = new DetailList();
        public DetailList J2 { get; } = new DetailList();

        [JsonIgnore]
        public LaboriousDetailList J12 { get; } = new LaboriousDetailList();
        [JsonIgnore]
        public LaboriousDetailList J21 { get; } = new LaboriousDetailList();

        public Chain J12Chain { get; set; }
        public Chain J21Chain { get; set; }
    }
}
