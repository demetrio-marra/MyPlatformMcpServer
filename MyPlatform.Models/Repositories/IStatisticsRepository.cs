using MyPlatformModels.Models;

namespace MyPlatformModels.Repositories
{
    public interface IStatisticsRepository
    {
        Task<IEnumerable<StatisticsResult>> GetAverageDurationAsync(
            DateTime? queryDateFrom = null,
            DateTime? queryDateTo = null,
            Companies? company = null,
            Families? family = null,
            Products? product = null,
            enumStatisticType? provisioningPhase = null,
            MyPlatform_Statistics_DataPartitioning dataPartitioning = MyPlatform_Statistics_DataPartitioning.Month,
            AclModel? userAcl = null);
    }
}
