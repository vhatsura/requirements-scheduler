namespace RequirementsScheduler.BLL.Model
{
    public sealed class ExperimentReport
    {
        public int Id { get; set; }
        public int ExperimentId { get; set; }

        public int Stop1 { get; set; }
        public int Stop2 { get; set; }
        public int Stop3 { get; set; }
        public int Stop4 { get; set; }

        public double ExecutionTime { get; set; }
        public float DeltaCmaxAverage { get; set; }
        public float DeltaCmaxMax { get; set; }
        public int ConflictsAmount { get; set; }
        public int ConflictsResolutionAmount { get; set; }
    }
}
