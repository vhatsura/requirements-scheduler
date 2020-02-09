using System.Threading;
using RequirementsScheduler.DAL.Model;

namespace RequirementsScheduler.DAL.Repository
{
    public class UsersInMemoryRepository : InMemoryRepository<User, int>
    {
        private int _id = 3;

        static UsersInMemoryRepository()
        {
            AddDefaultValue(
                new User {Id = 1, Username = "admin", Password = "admin", Role = "admin"},
                new User {Id = 2, Username = "user", Password = "user", Role = "user"}
            );
        }

        protected override int NextId => Interlocked.Increment(ref _id);
    }
}