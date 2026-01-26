using MyPlatformModels.Exceptions;
using MyPlatformModels.Models;
using MyPlatformModels.Repositories;
using MyPlatformModels.Services;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace MyPlatformModels
{
    public class StatisticsService : IStatisticsService
    {
        private readonly IStatisticsRepository _repository;
        private readonly IACLService _aclService;
        private readonly ILogger<StatisticsService> _logger;

        private const int StatisticsServiceRowCountLimit = 31;
        private const string StatisticsServiceRowCountLimitErrorMessage = "The requested statistics exceed the maximum allowed size. Please narrow down your filter parameters.";


        public StatisticsService(
            IStatisticsRepository repository,
            IACLService aclService,
            ILogger<StatisticsService> logger)
        {
            _repository = repository;
            _aclService = aclService;
            _logger = logger;
        }

        public async Task<IEnumerable<StatisticsResult>> GetAsync(
            DateTime queryDateFrom, 
            DateTime queryDateTo, 
            Products product, 
            MyPlatform_Statistics_DataPartitioning dataPartitioning, 
            Companies? company = null, 
            Families? family = null, 
            enumStatisticType? provisioningPhase = null)
        {
            // Stimiamo il numero di righe che la query restuituirà valutando soltanto il range di date e il partizionamento
            var estimatedRowCount = GetEstimatedRowCountPerProductAsync(
                queryDateFrom, queryDateTo, dataPartitioning);

            if (estimatedRowCount > StatisticsServiceRowCountLimit)
            {
                throw new UnprocessableRequestException(StatisticsServiceRowCountLimitErrorMessage);
            }

            _logger.LogDebug("Getting average duration statistics with filters - DateFrom: {DateFrom}, DateTo: {DateTo}, Company: {Company}, Family: {Family}, Product: {Product}, ProvisioningPhase: {ProvisioningPhase}, DataPartitioning: {DataPartitioning}",
                queryDateFrom, queryDateTo, company, family, product, provisioningPhase, dataPartitioning);
                
            var userAcl = await _aclService.GetUserAclAsync();
            var result = await _repository.GetAverageDurationAsync(
                queryDateFrom, queryDateTo, company, family, product, provisioningPhase, dataPartitioning, userAcl);

            // La query potrebbe restituire più righe del previsto a causa dei filtri su provisioningPhase.
            if (result.Count() > StatisticsServiceRowCountLimit)
            {
                throw new UnprocessableRequestException(StatisticsServiceRowCountLimitErrorMessage);
            }

            // results must contain a single company and family, otherwise raise an unprocessableentityexception
            if (result.Select(r => r.Company).Distinct().Count() > 1 ||
                result.Select(r => r.Family).Distinct().Count() > 1)
            {
                throw new UnprocessableRequestException("The requested statistics contain multiple companies or families. Please narrow down your filter parameters.");
            }

            _logger.LogInformation("Retrieved {Count} statistics records", result.Count());
            return result;
        }


        #region private

        private int GetEstimatedRowCountPerProductAsync(
           DateTime dateFrom,
           DateTime dateTo,
           MyPlatform_Statistics_DataPartitioning dataPartitioning)
        {
            // Calculate the number of time periods based on partitioning
            int periodCount = dataPartitioning switch
            {
                MyPlatform_Statistics_DataPartitioning.Year => CalculateYearsBetween(dateFrom, dateTo),
                MyPlatform_Statistics_DataPartitioning.Month => CalculateMonthsBetween(dateFrom, dateTo),
                MyPlatform_Statistics_DataPartitioning.Day => CalculateDaysBetween(dateFrom, dateTo),
                _ => 1 // Fallback for None or unexpected values
            };

            // Total estimated rows = periods × phases
            var estimatedRows = periodCount;

            _logger.LogDebug(
                "Estimated row count calculation: {PeriodCount} periods = {EstimatedRows} rows (partitioning: {DataPartitioning}, date range: {DateFrom} to {DateTo})",
                periodCount, estimatedRows, dataPartitioning, dateFrom, dateTo);

            return estimatedRows;
        }

        private static int CalculateYearsBetween(DateTime dateFrom, DateTime dateTo)
        {
            var years = dateTo.Year - dateFrom.Year;
            return years > 0 ? years : 1;
        }

        private static int CalculateMonthsBetween(DateTime dateFrom, DateTime dateTo)
        {
            var months = ((dateTo.Year - dateFrom.Year) * 12) + dateTo.Month - dateFrom.Month;
            return months > 0 ? months : 1;
        }

        private static int CalculateDaysBetween(DateTime dateFrom, DateTime dateTo)
        {
            var days = (dateTo - dateFrom).Days + 1; // +1 to include both start and end dates
            return days > 0 ? days : 1;
        }

        #endregion
    }
}
