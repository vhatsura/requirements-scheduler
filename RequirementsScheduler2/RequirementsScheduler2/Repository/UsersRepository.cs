using RequirementsScheduler2.Models;

namespace RequirementsScheduler2.Repository
{
    public class UsersRepository: Repository<User>
    {
        static UsersRepository()
        {
            ModelsCollection.Add(new User() {Id = 1, Username = "admin", Password = "admin", IsAdmin = true });
            ModelsCollection.Add(new User() { Id = 2, Username = "user", Password = "user", IsAdmin = false });
        }
    }
}
