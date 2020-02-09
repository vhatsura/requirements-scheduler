using System;
using LinqToDB.Mapping;
using RequirementsScheduler.DAL.Repository;

namespace RequirementsScheduler.DAL.Model
{
    public class ExperimentFailure : IRepositoryModel<int>
    {
        public Guid ExperimentId { get; set; }

        public string ErrorMessage { get; set; }

        [PrimaryKey] [Identity] public int Id { get; set; }
    }
}