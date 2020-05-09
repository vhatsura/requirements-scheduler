using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using AutoMapper;
using RequirementsScheduler.BLL.Model;
using RequirementsScheduler.Core.Service;
using RequirementsScheduler.DAL.Repository;
using Experiment = RequirementsScheduler.DAL.Model.Experiment;

namespace RequirementsScheduler.BLL.Service
{
    public class ExperimentsService : IExperimentsService
    {
        public ExperimentsService(
            IMapper mapper,
            IUserService userService,
            IRepository<Experiment, Guid> repository)
        {
            Mapper = mapper;
            UsersService = userService;
            Repository = repository;
        }

        protected IRepository<Experiment, Guid> Repository { get; }
        private IUserService UsersService { get; }
        private IMapper Mapper { get; }

        public IEnumerable<Model.Experiment> GetAll(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                throw new ArgumentException();
            }

            if (username == "worker")
            {
                return Repository
                    .Get()
                    .Select(e => Mapper.Map<Model.Experiment>(e));
            }

            var user = UsersService.GetByUserName(username);
            if (user != null)
            {
                return Repository
                    .Get(experiment => experiment.UserId == user.Id)
                    .Select(e => Mapper.Map<Model.Experiment>(e));
            }

            return Enumerable.Empty<Model.Experiment>();
        }

        public IEnumerable<Model.Experiment> GetByStatus(ExperimentStatus status, string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                throw new ArgumentException();
            }

            if (username == "worker")
            {
                return Repository
                    .Get(experiment => experiment.Status == (int) status)
                    .Select(e => Mapper.Map<Model.Experiment>(e));
            }

            var user = UsersService.GetByUserName(username);
            if (user != null)
            {
                return Repository
                    .Get(experiment => experiment.Status == (int) status &&
                                       experiment.UserId == user.Id)
                    .Select(e => Mapper.Map<Model.Experiment>(e));
            }

            return Enumerable.Empty<Model.Experiment>();
        }

        public Model.Experiment AddExperiment(Model.Experiment value, string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                throw new ArgumentException();
            }

            var user = UsersService.GetByUserName(username);

            if (user == null)
            {
                return null;
            }

            value.UserId = user.Id;
            value.Created = DateTime.UtcNow;
            var dalValue = Mapper.Map<Experiment>(value);
            return Mapper.Map<Model.Experiment>(Repository.Add(dalValue));
        }

        public Model.Experiment Get(Guid experimentId, string username,
            params Expression<Func<Model.Experiment, object>>[] membersToLoad)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                throw new ArgumentException();
            }

            if (username == "worker")
            {
                return Mapper.Map<Model.Experiment>(Repository.Get(experimentId));
            }

            var user = UsersService.GetByUserName(username);
            if (user != null)
                //todo think about security
            {
                return Mapper.Map<Model.Experiment>(
                    Repository.GetWith(experimentId, e => e.Result)
                );
            }

            return null;
        }
    }
}
