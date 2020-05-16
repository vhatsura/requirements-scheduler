using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using RequirementsScheduler.DAL.Model;
using RequirementsScheduler.DAL.Repository;

namespace RequirementsScheduler.Core.Service
{
    public sealed class UserService : IUserService
    {
        public UserService(IMapper mapper, IRepository<User, int> repository)
        {
            Mapper = mapper;
            Repository = repository;
        }

        private IRepository<User, int> Repository { get; }
        private IMapper Mapper { get; }

        public IEnumerable<BLL.Model.User> GetAllUsers()
        {
            return Repository
                .Get()
                .Select(user => Mapper.Map<BLL.Model.User>(user));
        }

        public BLL.Model.User GetUserById(int id) =>
            Mapper
                .Map<BLL.Model.User>(Repository.Get(id));

        public BLL.Model.User GetByUserName(string username)
        {
            return Mapper.Map<BLL.Model.User>(
                Repository
                    .Get(u => u.Username == username)
                    .FirstOrDefault());
        }

        public bool AddUser(BLL.Model.User value)
        {
            var existedUser = GetByUserName(value.Username);
            if (existedUser != null)
            {
                return false;
            }

            Repository.Add(Mapper.Map<User>(value));
            return true;
        }
    }
}
