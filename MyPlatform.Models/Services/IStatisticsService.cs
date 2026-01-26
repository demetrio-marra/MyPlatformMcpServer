using MyPlatformModels.Models;

namespace MyPlatformModels.Services
{
    public interface IStatisticsService
    {
        Task<IEnumerable<StatisticsResult>> GetAsync(
            DateTime queryDateFrom,
            DateTime queryDateTo,
            Products product,
            MyPlatform_Statistics_DataPartitioning dataPartitioning,
            Companies? company = null,
            Families? family = null,
            enumStatisticType? provisioningPhase = null);            
    }
}