using System;
using LinqToDB.Mapping;
using RequirementsScheduler.DAL.Repository;

namespace RequirementsScheduler.DAL.Model
{
    [Table("ExperimentsFailures")]
    public class ExperimentFailure
    {
        [Column]
        public Guid ExperimentId { get; set; }

        [Column]
        public string ErrorMessage { get; set; }
    }
}