using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using RequirementsScheduler.BLL.Model;
using RequirementsScheduler.Core.Service;
using RequirementsScheduler.DAL.Repository;

namespace RequirementsScheduler.BLL.Service
{
    public class ExperimentsService : IExperimentsService
    {
        protected IRepository<DAL.Model.Experiment, Guid> Repository { get; }
        private IUserService UsersService { get; }
        private IMapper Mapper { get; }

        public ExperimentsService(
            IMapper mapper,
            IUserService userService,
            IRepository<DAL.Model.Experiment, Guid> repository)
        {
            Mapper = mapper;
            UsersService = userService;
            Repository = repository;
        }

        public IEnumerable<Experiment> GetAll(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentException();

            if (username == "worker")
            {
                return Repository
                    .Get()
                    .Select(e => Mapper.Map<BLL.Model.Experiment>(e));
            }
            var user = UsersService.GetByUserName(username);
            if (user != null)
            {
                return Repository
                    .Get(experiment => experiment.UserId == user.Id)
                    .Select(e => Mapper.Map<BLL.Model.Experiment>(e));
            }

            return Enumerable.Empty<Experiment>();
        }

        public IEnumerable<Experiment> GetByStatus(ExperimentStatus status, string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentException();

            if (username == "worker")
            {
                return Repository
                    .Get(experiment => experiment.Status == (int) status)
                    .Select(e => Mapper.Map<BLL.Model.Experiment>(e));
            }

            var user = UsersService.GetByUserName(username);
            if (user != null)
            {
                return Repository
                    .Get(experiment => experiment.Status == (int)status &&
                                       experiment.UserId == user.Id)
                     .Select(e => Mapper.Map<BLL.Model.Experiment>(e));
            }
            return Enumerable.Empty<Experiment>();
        }

        public Experiment AddExperiment(Experiment value, string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentException();

            var user = UsersService.GetByUserName(username);

            if (user == null)
            {
                return null;
            }

            value.UserId = user.Id;
            value.Created = DateTime.UtcNow;
            var dalValue = Mapper.Map<DAL.Model.Experiment>(value);
            return Mapper.Map<BLL.Model.Experiment>(Repository.Add(dalValue));
        }

        public Experiment Get(Guid experimentId, string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentException();

            if (username == "worker")
            {
                return Mapper.Map<Experiment>(Repository.Get(experimentId));
            }
            var user = UsersService.GetByUserName(username);
            if (user != null)
            {
                //todo think about security
                return Mapper.Map<Experiment>(
                    Repository
                    .Get(experimentId));
            }

            return null;
        }
    }
}
