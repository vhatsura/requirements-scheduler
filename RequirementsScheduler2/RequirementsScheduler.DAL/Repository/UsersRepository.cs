using RequirementsScheduler.Core.Model;

namespace RequirementsScheduler.DAL.Repository
{
    public class UsersRepository : Repository<User>
    {
        static UsersRepository()
        {
            AddDefaultValue(
                new User() { Username = "admin", Password = "admin", IsAdmin = true },
                new User() { Username = "user", Password = "user", IsAdmin = false }
            );
        }
    }
}
