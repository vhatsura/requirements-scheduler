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
            CreateMap<BLL.Model.Experiment, DAL.Model.Experiment>();

            CreateMap<DAL.Model.ExperimentResult, BLL.Model.ExperimentReport>()
                .ForMember(dest => dest.N, opt => opt.MapFrom(src => src.Experiment.TestsAmount))
                .ForMember(dest => dest.N1Percentage, opt => opt.MapFrom(src => src.Experiment.N1))
                .ForMember(dest => dest.N2Percentage, opt => opt.MapFrom(src => src.Experiment.N2))
                .ForMember(dest => dest.N12Percentage, opt => opt.MapFrom(src => src.Experiment.N12))
                .ForMember(dest => dest.N21Percentage, opt => opt.MapFrom(src => src.Experiment.N21))
                .ForMember(dest => dest.ABorder, opt => opt.MapFrom(src => src.Experiment.MinPercentageFromA))
                .ForMember(dest => dest.BBorder, opt => opt.MapFrom(src => src.Experiment.MaxPercentageFromA))
                .ForMember(dest => dest.RequirementsAmount, opt => opt.MapFrom(src => src.Experiment.RequirementsAmount));

            CreateMap<BLL.Model.ExperimentReport, DAL.Model.ExperimentResult>();
        }
    }
}
