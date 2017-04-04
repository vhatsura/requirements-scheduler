using System.Threading;

namespace RequirementsScheduler.DAL.Repository
{
    public class UsersInMemoryRepository : InMemoryRepository<DAL.Model.User, int>
    {
        static UsersInMemoryRepository()
        {
            AddDefaultValue(
                new DAL.Model.User() { Id = 1, Username = "admin", Password = "admin", Role = "admin" },
                new DAL.Model.User() { Id = 2, Username = "user", Password = "user", Role = "user"}
            );
        }

        private int id = 3;

        protected override int NextId
        {
            get
            {
                var rId = id;
                Interlocked.Increment(ref rId);
                return rId;
            }
        }
    }
}
