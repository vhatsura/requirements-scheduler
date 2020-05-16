using LinqToDB.Mapping;
using RequirementsScheduler.DAL.Repository;

namespace RequirementsScheduler.DAL.Model
{
    public sealed class User : IRepositoryModel<int>
    {
        public string Username { get; set; }

        public string Password { get; set; }

        public string Role { get; set; }

        [PrimaryKey]
        [Identity]
        public int Id { get; set; }
    }
}
