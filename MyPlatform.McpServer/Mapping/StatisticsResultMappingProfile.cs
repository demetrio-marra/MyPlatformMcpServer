using AutoMapper;
using MyPlatformModels.Models;
using MyPlatformMcpServer.DTOs;

namespace MyPlatformMcpServer.Mapping
{
    public class StatisticsResultMappingProfile : Profile
    {
        public StatisticsResultMappingProfile()
        {
            // Map between StatisticsResult and PerformanceStatisticsResultDTO
            CreateMap<StatisticsResult, PerformanceStatisticsResultDTO>()
                .ForMember(dest => dest.ResultDateFrom, opt => opt.MapFrom(src => src.ResultDateFrom))
                .ForMember(dest => dest.ResultDateTo, opt => opt.MapFrom(src => src.ResultDateTo))
                .ForMember(dest => dest.ProvisioningPhase, opt => opt.MapFrom(src => src.ProvisioningPhase))
                .ForMember(dest => dest.UserAverageElapsedInterval, opt => opt.MapFrom(src => src.UserAverageElapsedInterval));

            CreateMap<StatisticsResult, StatisticsRatesResultDTO>()
                .ForMember(dest => dest.ResultDateFrom, opt => opt.MapFrom(src => src.ResultDateFrom))
                .ForMember(dest => dest.ResultDateTo, opt => opt.MapFrom(src => src.ResultDateTo))
                .ForMember(dest => dest.ProvisioningPhase, opt => opt.MapFrom(src => src.ProvisioningPhase))
                .ForMember(dest => dest.PercentangeOfSuccess, opt => opt.MapFrom(src => 
                    src.Ok.HasValue && src.Total.HasValue && src.Total != 0
                        ? (decimal?)((src.Ok.Value / (decimal)src.Total.Value) * 100)
                        : null))
                .ForMember(dest => dest.CumulativeKo, opt => opt.MapFrom(src => src.CumulativeKo));

            // Added mapping for CompleteStatisticsResultDTO (includes Total)
            CreateMap<StatisticsResult, StatisticsResultDTO>()
                .ForMember(dest => dest.ResultDateFrom, opt => opt.MapFrom(src => src.ResultDateFrom))
                .ForMember(dest => dest.ResultDateTo, opt => opt.MapFrom(src => src.ResultDateTo))
                .ForMember(dest => dest.ProvisioningPhase, opt => opt.MapFrom(src => src.ProvisioningPhase))
                .ForMember(dest => dest.Ok, opt => opt.MapFrom(src => src.Ok))
                .ForMember(dest => dest.Ko, opt => opt.MapFrom(src => src.Ko))
                .ForMember(dest => dest.CumulativeKo, opt => opt.MapFrom(src => src.CumulativeKo))
                .ForMember(dest => dest.Total, opt => opt.MapFrom(src => src.Total));
        }
    }
}