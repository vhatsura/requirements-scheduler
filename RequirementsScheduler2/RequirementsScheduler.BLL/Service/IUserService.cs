using System.Collections.Generic;
using RequirementsScheduler.BLL.Model;

namespace RequirementsScheduler.Core.Service
{
    public interface IUserService
    {
        IEnumerable<User> GetAllUsers();
        User GetUserById(int id);
        User GetByUserName(string username);
        bool AddUser(User value);
    }
}
