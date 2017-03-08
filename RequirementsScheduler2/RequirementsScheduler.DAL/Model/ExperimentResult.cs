using System;
using LinqToDB.Mapping;
using RequirementsScheduler.DAL.Repository;

namespace RequirementsScheduler.DAL.Model
{
    public class ExperimentResult : IRepositoryModel<Guid>
    {
        [PrimaryKey, Identity]
        public Guid Id { get; set; }

        public int Stop1Percentage { get; set; }
        public int Stop2Percentage { get; set; }
        public int Stop12Percentage { get; set; }
        public int Stop21Percentage { get; set; }
        public TimeSpan ExecutionTime { get; set; }

        public Guid ExperimentId { get; set; }
        [Association(ThisKey = nameof(ExperimentId), OtherKey = nameof(Model.Experiment.Id))]
        public Experiment Experiment { get; set; }
    }
}
