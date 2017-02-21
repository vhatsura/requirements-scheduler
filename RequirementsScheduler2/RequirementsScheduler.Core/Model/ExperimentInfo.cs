using System.Collections.Generic;

namespace RequirementsScheduler.Core.Model
{
    public class ProcessingTime
    {
        public float A { get; set; }
        public float B { get; set; }
    }
    public class ExperimentInfo
    {
        public List<ProcessingTime> First { get; set; }
        public List<ProcessingTime> Second { get; set; }
        public List<ProcessingTime> FirstSecond { get; set; }
        public List<ProcessingTime> SecondFirst { get; set; }
    }
}
