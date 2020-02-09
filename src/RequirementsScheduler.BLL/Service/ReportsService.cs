using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using RequirementsScheduler.BLL.Model;
using RequirementsScheduler.DAL.Model;
using RequirementsScheduler.DAL.Repository;
using Experiment = RequirementsScheduler.DAL.Model.Experiment;

namespace RequirementsScheduler.BLL.Service
{
    public class ReportsService : IReportsService
    {
        public ReportsService(
            IMapper mapper,
            IRepository<ExperimentResult, int> resultRepository,
            IRepository<Experiment, Guid> experimentRepository)
        {
            Mapper = mapper;
            ResultRepository = resultRepository;
            ExperimentRepository = experimentRepository;
        }

        private IMapper Mapper { get; }

        protected IRepository<ExperimentResult, int> ResultRepository { get; }
        protected IRepository<Experiment, Guid> ExperimentRepository { get; }

        public IEnumerable<ExperimentReport> GetAll()
        {
            return ResultRepository
                .GetWith(e => e.Experiment)
#if IN_MEMORY
                .Select(r => {
                    r.Experiment = ExperimentRepository.Get(r.ExperimentId);
                    return r;
                    })
#endif
                .Select(r => Mapper.Map<ExperimentReport>(r));
        }

        public void Save(ExperimentReport report)
        {
            ResultRepository.Add(Mapper.Map<ExperimentResult>(report));
        }
    }
}