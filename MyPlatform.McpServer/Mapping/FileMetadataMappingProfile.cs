using AutoMapper;
using MyPlatformMcpServer.DTOs;
using MyPlatformModels.Models;

namespace MyPlatformMcpServer.Mapping
{
    public class FileMetadataMappingProfile : Profile
    {
        public FileMetadataMappingProfile()
        {
            CreateMap<FileMetadata, FileMetadataDTO>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.FileName, opt => opt.MapFrom(src => src.FileName))
                .ForMember(dest => dest.ContentType, opt => opt.MapFrom(src => src.ContentType))
                .ForMember(dest => dest.FileSizeBytes, opt => opt.MapFrom(src => src.FileSizeBytes))
                .ForMember(dest => dest.FileExtension, opt => opt.MapFrom(src => src.FileExtension))
                .ForMember(dest => dest.UploadedAt, opt => opt.MapFrom(src => src.UploadedAt))
                .ForMember(dest => dest.Metadata, opt => opt.MapFrom(src => src.Metadata));
        }
    }
}
