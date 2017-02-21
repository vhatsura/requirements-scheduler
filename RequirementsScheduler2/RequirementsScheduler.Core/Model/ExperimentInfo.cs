using System.Collections.Generic;

namespace RequirementsScheduler.Core.Model
{
    public class ProcessingTime
    {
        public double A { get; }
        public double B { get; }

        public ProcessingTime(double a, double b)
        {
            A = a;
            B = b;
        }
    }

    public enum ResultType
    {
        STOP1_1
    }

    public class ResultInfo
    {
        public ResultType Type { get; set; }
    }

    public class ExperimentInfo
    {
        public ResultInfo Result { get; private set; } = new ResultInfo();

        public List<ProcessingTime> First { get; } = new List<ProcessingTime>();
        public List<ProcessingTime> Second { get; } = new List<ProcessingTime>();

        public ExperimentInfo1 FirstSecond { get; } = new ExperimentInfo1();
        public ExperimentInfo1 SecondFirst { get; } = new ExperimentInfo1();
    }

    public class ExperimentInfo1
    {
        public bool IsOptimized { get; set; }

        public List<ProcessingTime> First { get; } = new List<ProcessingTime>();
        public List<ProcessingTime> Second { get; } = new List<ProcessingTime>();
    }
}
