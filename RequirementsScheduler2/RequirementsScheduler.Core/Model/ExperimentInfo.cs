using System.Collections.Generic;
using System.Collections.ObjectModel;

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

    public class Detail
    {
        public ProcessingTime Time { get; }

        public Detail(double a, double b)
        {
            Time = new ProcessingTime(a, b);
        }
    }

    public interface IMachine
    {
        bool IsOptimized { get; }
    }

    public class DetailList : List<Detail>, IMachine
    {
        public bool IsOptimized => true;
    }

    public class LaboriousDetail
    {
        public Detail OnFirst { get; }
        public Detail OnSecond { get; }

        public LaboriousDetail(Detail onFirst, Detail onSecond)
        {
            OnFirst = onFirst;
            OnSecond = onSecond;
        }
    }

    public enum ResultType
    {
        STOP1_1,
        STOP1_2
    }

    public class ResultInfo
    {
        public ResultType Type { get; set; }
    }

    public class ExperimentInfo : IMachine
    {
        public bool IsOptimized => 
            J1.IsOptimized && 
            J2.IsOptimized && 
            J12.IsOptimized && 
            J21.IsOptimized;

        public ResultInfo Result { get; private set; } = new ResultInfo();
        
        public DetailList J1 { get; } = new DetailList();
        public DetailList J2 { get; } = new DetailList();

        public LaboriousDetailList J12 { get; } = new LaboriousDetailList();
        public LaboriousDetailList J21 { get; } = new LaboriousDetailList();
    }

    public class LaboriousDetailList : List<LaboriousDetail>, IMachine
    {
        public bool IsOptimized { get; set; }
    }
}
