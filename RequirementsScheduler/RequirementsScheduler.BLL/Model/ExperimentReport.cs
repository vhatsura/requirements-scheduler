using System;

namespace RequirementsScheduler.BLL.Model
{
    public sealed class ReportInfo
    {
        public int OfflineResolvedConflictAmount { get; set; }
        public int OnlineResolvedConflictAmount { get; set; }
        public int OnlineUnResolvedConflictAmount { get; set; }

        public float Stop1Percentage { get; set; }
        public float Stop2Percentage { get; set; }
        public float Stop3Percentage { get; set; }
        public float Stop4Percentage { get; set; }

        public float DeltaCmaxMax { get; set; }
        public float DeltaCmaxAverage { get; set; }

        public TimeSpan OnlineExecutionTime { get; set; }
    }
    
    public sealed class ExperimentReport
    {
        public int Id { get; set; }
        public Guid ExperimentId { get; set; }

        public int N { get; set; }
        public int RequirementsAmount { get; set; }

        public int N1Percentage { get; set; }
        public int N2Percentage { get; set; }
        public int N12Percentage { get; set; }
        public int N21Percentage { get; set; }

        public int ABorder { get; set; }
        public int BBorder { get; set; }
        public int MinPercentageFromA { get; set; }
        public int MaxPercentageFromA { get; set; }

        public int OfflineResolvedConflictAmount { get; set; }
        public int OnlineResolvedConflictAmount { get; set; }
        public int OnlineUnResolvedConflictAmount { get; set; }

        public float Stop1Percentage { get; set; }
        public float Stop2Percentage { get; set; }
        public float Stop3Percentage { get; set; }
        public float Stop4Percentage { get; set; }

        public float DeltaCmaxMax { get; set; }
        public float DeltaCmaxAverage { get; set; }

        public TimeSpan OnlineExecutionTime { get; set; }
    }
}
