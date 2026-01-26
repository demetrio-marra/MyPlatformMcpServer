using AutoMapper;
using MyPlatformModels.Helpers;
using MyPlatformModels.Models;
using MyPlatformMcpServer.DTOs;

namespace MyPlatformMcpServer.Mapping
{
    public class MyPermissionsMappingProfile : Profile
    {
        public MyPermissionsMappingProfile()
        {
            // Map between AclModelDetail and MyPermissionsDTO
            CreateMap<AclModelDetail, MyPermissionsDTO>()
                .ForMember(dest => dest.Company, opt =>
                {
                    opt.PreCondition(src => src.Company.HasValue);
                    opt.MapFrom(src => MyPlatformEnumHelper.EnumToLabel<Companies>(src.Company!.Value));
                })
                .ForMember(dest => dest.Family, opt =>
                {
                    opt.PreCondition(src => src.Family.HasValue);
                    opt.MapFrom(src => MyPlatformEnumHelper.EnumToLabel<Families>(src.Family!.Value));
                })
                .ForMember(dest => dest.Product, opt =>
                {
                    opt.PreCondition(src => src.Product.HasValue);
                    opt.MapFrom(src => MyPlatformEnumHelper.EnumToLabel<Products>(src.Product!.Value));
                });
        }
    }
}
