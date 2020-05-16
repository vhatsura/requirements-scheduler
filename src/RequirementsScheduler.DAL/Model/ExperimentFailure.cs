using System;
using LinqToDB.Mapping;

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
