using AutoMapper;
using MyPlatformModels.Exceptions;
using MyPlatformModels.Helpers;
using MyPlatformModels.Models;
using MyPlatformModels.Services;
using System.Globalization;

namespace MyPlatformMcpServer.Tools.Statistics
{
    public abstract class BaseStatisticsTools
    {
        // Shared description constants
        protected const string Desc_QueryDateFrom = "Start date filter for the search range (inclusive), in ISO8601 format without a time component (e.g., \"2025-10-20\")";
        protected const string Desc_QueryDateTo = "End date filter for the search range (inclusive), in ISO8601 format without a time component (e.g., \"2025-12-20\")";
        protected const string Desc_Company = "Name of the company filter";
        protected const string Desc_Family = "Product's family name filter";
        protected const string Desc_Product = "Product's name filter";
        protected const string Desc_ProvisioningPhase = "Product lifecycle phase filter";
        protected const string Desc_DataPartitioning = "Group data by time period";

        // Shared dependencies
        protected readonly IStatisticsService _statisticsService;
        protected readonly ICompanyInfoService _companyInfoService;
        protected readonly IMapper _mapper;
        protected readonly ILogger _logger;

        protected BaseStatisticsTools(
            IStatisticsService statisticsService,
            ICompanyInfoService companyInfoService,
            IMapper mapper,
            ILogger logger)
        {
            _statisticsService = statisticsService;
            _companyInfoService = companyInfoService;
            _mapper = mapper;
            _logger = logger;
        }

        protected async Task<IEnumerable<TDto>> GetStatisticsInternalAsync<TDto>(
            string queryDateFrom,
            string queryDateTo,
            Products product,
            MyPlatform_Statistics_DataPartitioning dataPartitioning,
            enumStatisticType? provisioningPhase,
            Companies? company,
            Families? family)
        {
            // Validate and parse required queryDateFrom parameter
            if (string.IsNullOrWhiteSpace(queryDateFrom))
            {
                _logger.LogWarning("Validation error in GetAsync: queryDateFrom is required");
                throw new CustomToolException("queryDateFrom parameter is required. Expected format: 'yyyy-MM-dd' (e.g., '2025-10-20')");
            }

            if (!DateTime.TryParseExact(queryDateFrom.Trim(), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dateFrom))
            {
                _logger.LogWarning("Validation error in GetAsync: invalid queryDateFrom format {QueryDateFrom}", queryDateFrom);
                throw new CustomToolException($"Invalid queryDateFrom format: '{queryDateFrom}'. Expected format: 'yyyy-MM-dd' (e.g., '2025-10-20')");
            }

            // Validate and parse required queryDateTo parameter
            if (string.IsNullOrWhiteSpace(queryDateTo))
            {
                _logger.LogWarning("Validation error in GetAsync: queryDateTo is required");
                throw new CustomToolException("queryDateTo parameter is required. Expected format: 'yyyy-MM-dd' (e.g., '2025-12-20')");
            }

            if (!DateTime.TryParseExact(queryDateTo.Trim(), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dateTo))
            {
                _logger.LogWarning("Validation error in GetAsync: invalid queryDateTo format {QueryDateTo}", queryDateTo);
                throw new CustomToolException($"Invalid queryDateTo format: '{queryDateTo}'. Expected format: 'yyyy-MM-dd' (e.g., '2025-12-20')");
            }

            // Validate date range
            if (dateFrom > dateTo)
            {
                _logger.LogWarning("Validation error in GetAsync: queryDateFrom {QueryDateFrom} is after queryDateTo {QueryDateTo}", dateFrom, dateTo);
                throw new CustomToolException("queryDateFrom cannot be after queryDateTo");
            }

            // Resolve company and family from product hierarchy if not provided
            Companies? companyEnum = company;
            Families? familyEnum = family;

            if (familyEnum == null || companyEnum == null)
            {
                IEnumerable<CompanyInfoProductHierarchyItem> hierarchyList;
                try
                {
                    hierarchyList = await _companyInfoService.GetProductHierarchyListAsync(product);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Validation error in GetAsync: failed to retrieve product hierarchy for product {Product}", product);
                    throw new CustomToolException($"Failed to retrieve product hierarchy for product: '{product}'. Contact support team.", ex);
                }

                if (!hierarchyList.Any())
                {
                    _logger.LogWarning("Validation error in GetAsync: no valid hierarchy found for product {Product}", product);
                    throw new CustomToolException($"No valid hierarchy found for product: '{product}'");
                }

                if (companyEnum.HasValue)
                {
                    hierarchyList = hierarchyList.Where(h => h.Company != null && h.Company.Equals(companyEnum.Value.ToString()))
                        .ToList();
                }

                if (familyEnum.HasValue)
                {
                    hierarchyList = hierarchyList.Where(h => h.Family != null && h.Family.Equals(familyEnum.Value.ToString()))
                        .ToList();
                }

                if (hierarchyList.Count() > 1)
                {
                    _logger.LogWarning("Validation error in GetAsync: ambiguous product hierarchy for product value {Product}", product);
                    throw new CustomToolException($"Ambiguous product hierarchy for product value: '{product}'. Please specify both family and company parameters to disambiguate from this list: {System.Text.Json.JsonSerializer.Serialize(hierarchyList)}");
                }

                var hierarchy = hierarchyList.First();
                if (familyEnum == null)
                {
                    familyEnum = MyPlatformEnumHelper.LabelToEnum<Families>(hierarchy.Family);
                }
                if (companyEnum == null)
                {
                    companyEnum = MyPlatformEnumHelper.LabelToEnum<Companies>(hierarchy.Company);
                }
            }

            try
            {
                // Validate company-family-product hierarchy relationships
                await _companyInfoService.ValidateProductHierarchy(companyEnum, familyEnum, product);

                // Call the statistics service
                var results = await _statisticsService.GetAsync(
                    dateFrom,
                    dateTo,
                    product,
                    dataPartitioning,
                    companyEnum,
                    familyEnum,
                    provisioningPhase);

                var mappedResults = _mapper.Map<IEnumerable<TDto>>(results);

                _logger.LogInformation("Successfully retrieved statistics with {Count} results", mappedResults?.Count() ?? 0);

                return mappedResults;
            }
            catch (AgentIdNotFoundException ex)
            {
                _logger.LogWarning("Failed to retrieve statistics in GetAsync: {ErrorMessage}", ex.Message);
                throw new CustomToolException(ex.Message);
            }
            catch (UnprocessableRequestException ex)
            {
                _logger.LogWarning(ex, "Validation error in GetAsync: {ErrorMessage}", ex.Message);
                throw new CustomToolException("Validation error in Get: " + ex.Message, ex);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Validation error in GetAsync: {ErrorMessage}", ex.Message);
                throw new CustomToolException("Validation error in Get: " + ex.Message, ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetAsync: {ErrorMessage}", ex.Message);
                throw new CustomToolException("An error occurred while getting statistics.", ex);
            }
        }
    }
}
