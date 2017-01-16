using System;
using System.Collections.Generic;
using RequirementsScheduler.Core.Model;
using RequirementsScheduler.DAL.Repository;
using System.Linq;

namespace RequirementsScheduler.Core.Service
{
    public sealed class UserService : IUserService
    {
        private readonly IRepository<User> Repository = new UsersRepository();

        public IEnumerable<User> GetAllUsers()
        {
            return Repository.Get();
        }

        public User GetUserById(int id)
        {
            return Repository.Get(u => u.Id == id).FirstOrDefault();
        }

        public User GetByUserName(string username)
        {
            return Repository.Get(u => u.Username == username).FirstOrDefault();
        }

        public bool AddUser(User value)
        {
            var existedUser = this.GetByUserName(value.Username);
            if (existedUser != null)
                return false;

            Repository.Add(value);
            return true;
        }
    }
}
