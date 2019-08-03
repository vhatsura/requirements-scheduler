using System;
using LinqToDB.Mapping;
using RequirementsScheduler.DAL.Repository;

namespace RequirementsScheduler.DAL.Model
{
    public class ExperimentFailure : IRepositoryModel<int>
    {
        [PrimaryKey, Identity]
        public int Id { get; set; }

        public Guid ExperimentId { get; set; }

        public string ErrorMessage { get; set; }
    }
}
