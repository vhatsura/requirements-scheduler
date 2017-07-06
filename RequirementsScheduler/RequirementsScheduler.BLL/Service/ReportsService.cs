using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using RequirementsScheduler.BLL.Model;
using RequirementsScheduler.DAL.Repository;

namespace RequirementsScheduler.BLL.Service
{
    public class ReportsService: IReportsService
    {
        private IMapper Mapper { get; }

        protected IRepository<DAL.Model.ExperimentResult, int> ResultRepository { get; }
        protected IRepository<DAL.Model.Experiment, Guid> ExperimentRepository { get; }

        public ReportsService(
            IMapper mapper,
            IRepository<DAL.Model.ExperimentResult, int> resultRepository,
            IRepository<DAL.Model.Experiment, Guid> experimentRepository)
        {
            Mapper = mapper;
            ResultRepository = resultRepository;
            ExperimentRepository = experimentRepository;
        }
       
        public IEnumerable<ExperimentReport> GetAll()
        {
                return ResultRepository
                .GetWith(e => e.Experiment)
#if IN_MEMORY
                .Select(r => r.Experiment = ExperimentRepository.Get(r.ExperimentId))
#endif
                .Select(r => Mapper.Map<ExperimentReport>(r));
        }

        public void Save(ExperimentReport report)
        {
            ResultRepository.Add(Mapper.Map<DAL.Model.ExperimentResult>(report));
        }
    }
}
