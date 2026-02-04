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
            _logger.LogDebug("Getting average duration statistics with filters - DateFrom: {DateFrom}, DateTo: {DateTo}, Company: {Company}, Family: {Family}, Product: {Product}, ProvisioningPhase: {ProvisioningPhase}, DataPartitioning: {DataPartitioning}",
                queryDateFrom, queryDateTo, company, family, product, provisioningPhase, dataPartitioning);
                
            var userAcl = await _aclService.GetUserAclAsync();
            var result = await _repository.GetAverageDurationAsync(
                queryDateFrom, queryDateTo, company, family, product, provisioningPhase, dataPartitioning, userAcl);

            // results must contain a single company and family, otherwise raise an unprocessableentityexception
            if (result.Select(r => r.Company).Distinct().Count() > 1 ||
                result.Select(r => r.Family).Distinct().Count() > 1)
            {
                throw new UnprocessableRequestException("The requested statistics contain multiple companies or families. Please narrow down your filter parameters.");
            }

            _logger.LogInformation("Retrieved {Count} statistics records", result.Count());
            return result;
        }
    }
}
