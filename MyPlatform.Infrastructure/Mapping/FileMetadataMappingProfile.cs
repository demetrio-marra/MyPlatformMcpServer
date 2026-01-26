using AutoMapper;

namespace MyPlatformInfrastructure.Mapping
{
    public class FileMetadataMappingProfile : Profile
    {
        public FileMetadataMappingProfile()
        {
            CreateMap<MyPlatformInfrastructure.DTOs.FileMetadataDTO, MyPlatformModels.Models.FileMetadata>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.FileName, opt => opt.MapFrom(src => src.FileName))
                .ForMember(dest => dest.FileExtension, opt => opt.MapFrom(src => src.FileExtension))
                .ForMember(dest => dest.ContentType, opt => opt.MapFrom(src => src.ContentType))
                .ForMember(dest => dest.FileSizeBytes, opt => opt.MapFrom(src => src.FileSizeBytes))
                .ForMember(dest => dest.UploadedAt, opt => opt.MapFrom(src => src.UploadedAt))
                .ForMember(dest => dest.Metadata, opt => opt.MapFrom(src => src.Metadata));
        }
    }
}
