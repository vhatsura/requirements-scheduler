using AutoMapper;
using System;
using RequirementsScheduler.BLL.Model;

namespace RequirementsScheduler.BLL
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<DAL.Model.User, User>()
                .ForMember(dest => dest.Email, opt => opt.Ignore())
                .ReverseMap();

            CreateMap<DAL.Model.Experiment, Experiment>()
                .ForMember(dest => dest.Report, opt => opt.MapFrom(src => src.Result))
                .ForMember(dest => dest.BorderGenerationType, opt => opt.Ignore())
                .ForMember(dest => dest.PGenerationType, opt => opt.Ignore())
                .ForMember(dest => dest.Results, opt => opt.Ignore())
                .ReverseMap();

            CreateMap<DAL.Model.ExperimentResult, BLL.Model.ExperimentReport>()
                .ForMember(dest => dest.N, opt => opt.MapFrom(src => src.Experiment.TestsAmount))
                .ForMember(dest => dest.N1Percentage, opt => opt.MapFrom(src => src.Experiment.N1))
                .ForMember(dest => dest.N2Percentage, opt => opt.MapFrom(src => src.Experiment.N2))
                .ForMember(dest => dest.N12Percentage, opt => opt.MapFrom(src => src.Experiment.N12))
                .ForMember(dest => dest.N21Percentage, opt => opt.MapFrom(src => src.Experiment.N21))
                .ForMember(dest => dest.ABorder, opt => opt.MapFrom(src => src.Experiment.MinBoundaryRange))
                .ForMember(dest => dest.BBorder, opt => opt.MapFrom(src => src.Experiment.MaxBoundaryRange))
                .ForMember(dest => dest.MinPercentageFromA,
                    opt => opt.MapFrom(src => src.Experiment.MinPercentageFromA))
                .ForMember(dest => dest.MaxPercentageFromA,
                    opt => opt.MapFrom(src => src.Experiment.MaxPercentageFromA))
                .ForMember(dest => dest.RequirementsAmount,
                    opt => opt.MapFrom(src => src.Experiment.RequirementsAmount))
                .ForMember(
                    dest => dest.OnlineExecutionTime,
                    opt => opt.MapFrom(src => TimeSpan.FromSeconds(src.OnlineExecutionTime.TotalSeconds)))
                .ForMember(dest => dest.OfflineExecutionTime,
                    opt => opt.MapFrom(src => TimeSpan.FromSeconds(src.OfflineExecutionTime.TotalSeconds)))
                .ReverseMap();

            CreateMap<DAL.Model.ExperimentResult, ReportInfo>();
        }
    }
}