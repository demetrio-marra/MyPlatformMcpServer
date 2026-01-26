using AutoMapper;
using MyPlatformMcpServer.DTOs;
using MyPlatformModels.Services;
using ModelContextProtocol.Server;
using System.ComponentModel;

namespace MyPlatformMcpServer.Tools.Statistics;

[McpServerToolType]
public sealed class StatisticsTool : BaseStatisticsTools
{
    public StatisticsTool(
        IStatisticsService statisticsService,
        ICompanyInfoService companyInfoService,
        ILogger<StatisticsTool> logger,
        IMapper mapper)
        : base(statisticsService, companyInfoService, mapper, logger)
    {
    }

    [McpServerTool(Name = "MyPlatform_Statistics_Get",
        Idempotent = true, Destructive = false, OpenWorld = false, ReadOnly = true, UseStructuredContent = true),
        Description("Get provisioning processes statistics for a specific product using Company, Family, Product and ProvisioningPhase filters. Supports data partitioning.")]
    public Task<IEnumerable<StatisticsResultDTO>> GetAsync(
        [Description(Desc_QueryDateFrom)] string queryDateFrom,
        [Description(Desc_QueryDateTo)] string queryDateTo,
        [Description(Desc_Product)] string product,
        [Description(Desc_DataPartitioning)] string dataPartitioning,
        [Description(Desc_ProvisioningPhase)] string? provisioningPhase = null,
        [Description(Desc_Company)] string? company = null,
        [Description(Desc_Family)] string? family = null)
        => GetStatisticsInternalAsync<StatisticsResultDTO>(
            queryDateFrom,
            queryDateTo,
            product,
            dataPartitioning,
            provisioningPhase,
            company,
            family);
}
