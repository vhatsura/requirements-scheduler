using System;
using LinqToDB.Mapping;
using RequirementsScheduler.DAL.Repository;

namespace RequirementsScheduler.DAL.Model
{
    public class Experiment : IRepositoryModel<Guid>
    {
        public int TestsAmount { get; set; }
        public int RequirementsAmount { get; set; }
        public int N1 { get; set; }
        public int N2 { get; set; }
        public int N12 { get; set; }
        public int N21 { get; set; }
        public int MinBoundaryRange { get; set; }
        public int MaxBoundaryRange { get; set; }
        public int MinPercentageFromA { get; set; }
        public int MaxPercentageFromA { get; set; }

        public Distribution BorderGenerationType { get; set; }

        public Distribution PGenerationType { get; set; }

        public int Status { get; set; }

        public DateTime Created { get; set; }

        public int UserId { get; set; }

        [Association(ThisKey = nameof(UserId), OtherKey = nameof(Model.User.Id), CanBeNull = false)]
        public User User { get; set; }

        [Association(ThisKey = nameof(Id), OtherKey = nameof(ExperimentResult.ExperimentId), CanBeNull = true)]
        public ExperimentResult Result { get; set; }

        [PrimaryKey] [Identity] public Guid Id { get; set; }
    }
}