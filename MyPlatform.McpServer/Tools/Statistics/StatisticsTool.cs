using AutoMapper;
using MyPlatformMcpServer.DTOs;
using MyPlatformModels.Models;
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
        [Description(Desc_Product)] Products product,
        [Description(Desc_DataPartitioning)] MyPlatform_Statistics_DataPartitioning dataPartitioning,
        [Description(Desc_ProvisioningPhase)] enumStatisticType? provisioningPhase = null,
        [Description(Desc_Company)] Companies? company = null,
        [Description(Desc_Family)] Families? family = null)
        => GetStatisticsInternalAsync<StatisticsResultDTO>(
            queryDateFrom,
            queryDateTo,
            product,
            dataPartitioning,
            provisioningPhase,
            company,
            family);
}
