using AutoMapper;

namespace RequirementsScheduler.BLL
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<DAL.Model.User, BLL.Model.User>();
            CreateMap<BLL.Model.User, DAL.Model.User>();

            CreateMap<DAL.Model.Experiment, BLL.Model.Experiment>();
        }
    }
}
