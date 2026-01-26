using MyPlatformModels.Models;
using MyPlatformModels.Repositories;

namespace MyPlatformInfrastructure.Repositories
{
    public class MockStatisticsRepository : IStatisticsRepository
    {
        public Task<IEnumerable<StatisticsResult>> GetAverageDurationAsync(
            DateTime? queryDateFrom = null,
            DateTime? queryDateTo = null,
            Companies? company = null,
            Families? family = null,
            Products? product = null,
            enumStatisticType? provisioningPhase = null,
            MyPlatform_Statistics_DataPartitioning dataPartitioning = MyPlatform_Statistics_DataPartitioning.Month,
            AclModel? userAcl = null)
        {
            var results = new List<StatisticsResult>();

            var startDate = queryDateFrom ?? DateTime.Now.AddMonths(-3);
            var endDate = queryDateTo ?? DateTime.Now;

            var currentDate = startDate;
            while (currentDate <= endDate)
            {
                DateTime periodEnd;
                switch (dataPartitioning)
                {
                    case MyPlatform_Statistics_DataPartitioning.Day:
                        periodEnd = currentDate;
                        break;
                    case MyPlatform_Statistics_DataPartitioning.Month:
                        periodEnd = new DateTime(currentDate.Year, currentDate.Month, DateTime.DaysInMonth(currentDate.Year, currentDate.Month));
                        break;
                    case MyPlatform_Statistics_DataPartitioning.Year:
                        periodEnd = new DateTime(currentDate.Year, 12, 31);
                        break;
                    default:
                        periodEnd = new DateTime(currentDate.Year, currentDate.Month, DateTime.DaysInMonth(currentDate.Year, currentDate.Month));
                        break;
                }

                if (periodEnd > endDate)
                    periodEnd = endDate;

                var result = new StatisticsResult
                {
                    ResultDateFrom = DateOnly.FromDateTime(currentDate),
                    ResultDateTo = DateOnly.FromDateTime(periodEnd),
                    Company = company?.ToString() ?? "AllCompanies",
                    Family = family?.ToString() ?? "AllFamilies",
                    Product = product?.ToString() ?? "AllProducts",
                    ProvisioningPhase = provisioningPhase?.ToString() ?? "AllPhases",
                    Ok = Random.Shared.Next(50, 150),
                    Ko = Random.Shared.Next(0, 20),
                    CumulativeKo = Random.Shared.Next(0, 50),
                    Total = Random.Shared.Next(50, 170),
                    UserAverageElapsedInterval = Random.Shared.Next(30, 300)
                };

                results.Add(result);

                switch (dataPartitioning)
                {
                    case MyPlatform_Statistics_DataPartitioning.Day:
                        currentDate = currentDate.AddDays(1);
                        break;
                    case MyPlatform_Statistics_DataPartitioning.Month:
                        currentDate = currentDate.AddMonths(1);
                        break;
                    case MyPlatform_Statistics_DataPartitioning.Year:
                        currentDate = currentDate.AddYears(1);
                        break;
                    default:
                        currentDate = currentDate.AddMonths(1);
                        break;
                }
            }

            return Task.FromResult<IEnumerable<StatisticsResult>>(results);
        }
    }
}
