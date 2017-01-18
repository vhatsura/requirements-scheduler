using System;
using System.Collections.Generic;
using System.Linq;
using RequirementsScheduler.Core.Model;
using RequirementsScheduler.DAL.Repository;

namespace RequirementsScheduler.Core.Service
{
    public class ExperimentsService : IExperimentsService
    {
        private readonly IRepository<Experiment> ExperimentsRepository = new ExperimentsRepository();
        private readonly IUserService UsersService = new UserService();


        public IEnumerable<Experiment> GetAll(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentException();

            var user = UsersService.GetByUserName(username);
            if (user != null)
            {
                return ExperimentsRepository
                    .Get(experiment => experiment.UserId == user.Id);
            }

            return Enumerable.Empty<Experiment>();
        }

        public IEnumerable<Experiment> GetByStatus(ExperimentStatus status, string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentException();

            var user = UsersService.GetByUserName(username);
            if (user != null)
            {
                return ExperimentsRepository
                    .Get(experiment => experiment.Status == status &&
                     experiment.UserId == user.Id);
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
            return ExperimentsRepository.Add(value);
        }
    }
}
