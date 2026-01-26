using MyPlatformModels.Models;

namespace MyPlatformModels.Services
{
    public interface IChartService
    {
        Task<FileMetadata> GenerateChart(ChartRequest chartRequest);
    }
}
