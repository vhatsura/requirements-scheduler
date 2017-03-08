using System.Collections.Generic;
using RequirementsScheduler.DAL.Repository;
using System.Linq;
using AutoMapper;
using RequirementsScheduler.BLL.Model;

namespace RequirementsScheduler.Core.Service
{
    public sealed class UserService : IUserService
    {
        private IRepository<DAL.Model.User, int> Repository { get; }
        private IMapper Mapper { get; }

        public UserService(IMapper mapper, IRepository<DAL.Model.User, int> repository)
        {
            Mapper = mapper;
            Repository = repository;
        }

        public IEnumerable<User> GetAllUsers()
        {
            return Repository
                .Get()
                .Select(user => Mapper.Map<BLL.Model.User>(user));
        }

        public User GetUserById(int id)
        {
            return Mapper
                .Map<BLL.Model.User>(Repository.Get(id));
        }

        public User GetByUserName(string username)
        {
            return Mapper.Map<BLL.Model.User>(
                Repository
                    .Get(u => u.Username == username)
                    .FirstOrDefault());
        }

        public bool AddUser(User value)
        {
            var existedUser = this.GetByUserName(value.Username);
            if (existedUser != null)
                return false;

            Repository.Add(Mapper.Map<DAL.Model.User>(value));
            return true;
        }
    }
}
