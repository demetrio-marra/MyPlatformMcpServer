using AutoMapper;
using MyPlatformMcpServer.DTOs;
using MyPlatformModels.Models;
using MyPlatformModels.Services;
using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Text.Json;

namespace MyPlatformMcpServer.Tools;

/// <summary>
/// Tools for generating visual charts and graphs from data.
/// Provides capabilities to create bar charts, line charts, and pie charts with customizable styling.
/// </summary>
[McpServerToolType]
public sealed class ChartTools
{
    // Comprehensive parameter descriptions to help LLMs understand how to use the chart generation tool
    private const string Desc_ChartType = "Type of chart to generate. Valid values: 'bar' (vertical bar chart for comparing values across categories), 'line' (line chart for showing trends over time or continuous data), 'pie' (pie chart for showing proportional distribution of parts to a whole). Default is 'bar'.";
    
    private const string Desc_Title = "Title text displayed at the top of the chart. Use a descriptive title that clearly indicates what the chart represents. Example: 'Monthly Sales Revenue' or 'Product Performance Comparison'. Default is 'Chart'.";
    
    private const string Desc_XLabel = "Label for the X-axis (horizontal axis). For bar charts, this typically represents the category dimension. For line charts, this often represents time or sequential data. Optional - defaults to 'X' if not provided. Not applicable for pie charts.";
    
    private const string Desc_YLabel = "Label for the Y-axis (vertical axis). This typically represents the measured value or metric being displayed. Optional - defaults to 'Y' if not provided. Example: 'Revenue ($)', 'Count', 'Percentage'. Not applicable for pie charts.";
    
    private const string Desc_DataJson = "JSON array of data points for the chart. Each data point must be an object with 'label' (string) and 'value' (number) properties. Format: [{\"label\": \"Category1\", \"value\": 100}, {\"label\": \"Category2\", \"value\": 200}]. For bar charts: labels represent categories, values are bar heights. For line charts: labels represent X-axis points (can be sequential), values represent Y-axis measurements. For pie charts: labels represent slice names, values represent slice sizes (will be shown as proportions). Minimum 1 data point required.";

    private readonly IChartService _chartService;
    private readonly IMapper _mapper;
    private readonly ILogger<ChartTools> _logger;

    // JSON deserialization options for flexible property matching
    private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true,
        AllowTrailingCommas = true,
        ReadCommentHandling = JsonCommentHandling.Skip
    };

    public ChartTools(IChartService chartService, ILogger<ChartTools> logger, IMapper mapper)
    {
        _chartService = chartService;
        _logger = logger;
        _mapper = mapper;
    }

    /// <summary>
    /// Generates a chart image from provided data and uploads it to an external file server.
    /// Supports bar charts, line charts, and pie charts with automatic color assignment.
    /// Returns a reference to the uploaded file for later use.
    /// </summary>
    [McpServerTool(Name = "MyPlatform_Chart_GenerateChart",
        Idempotent = true, 
        Destructive = false, 
        OpenWorld = false, 
        ReadOnly = true,
        UseStructuredContent = true),
        Description("Generates a visual chart (bar, line, or pie) from data, uploads it to an external file server service and returns a reference to the uploaded file for later use. " +
                    "Use this tool when you need to create visual representations of data for analysis or presentation. " +
                    "Bar charts are ideal for comparing discrete categories, line charts for showing trends or time series, " +
                    "and pie charts for displaying proportional distributions where all parts sum to a meaningful whole.")]
    public async Task<FileMetadataDTO> GenerateChartAsync(
        [Description(Desc_ChartType)] string chartType = "bar",
        [Description(Desc_Title)] string title = "Chart",
        [Description(Desc_XLabel)] string? xLabel = null,
        [Description(Desc_YLabel)] string? yLabel = null,
        [Description(Desc_DataJson)] string dataJson = "[]")
    {
        try
        {
            // Validate and normalize chart type
            var normalizedChartType = chartType?.Trim().ToLower() ?? "bar";
            var validChartTypes = new[] { "bar", "line", "pie" };
            
            if (!validChartTypes.Contains(normalizedChartType))
            {
                _logger.LogWarning("Invalid chart type provided: {ChartType}", chartType);
                throw new CustomToolException(
                    $"Invalid chartType: '{chartType}'. Valid values are: {string.Join(", ", validChartTypes)}. " +
                    "Use 'bar' for category comparisons, 'line' for trends, or 'pie' for proportional distributions.");
            }

            // Parse and validate data JSON
            List<ChartData>? chartDataList;
            try
            {
                chartDataList = JsonSerializer.Deserialize<List<ChartData>>(dataJson, JsonOptions);
            }
            catch (JsonException ex)
            {
                _logger.LogWarning(ex, "Failed to parse dataJson: {DataJson}", dataJson);
                throw new CustomToolException(
                    "Invalid JSON format for dataJson. Expected an array of objects with 'label' (string) and 'value' (number) properties. " +
                    @"Example: [{""label"": ""Item1"", ""value"": 100}, {""label"": ""Item2"", ""value"": 200}]. " +
                    $"Error: {ex.Message}");
            }

            // Validate parsed data
            if (chartDataList == null || !chartDataList.Any())
            {
                _logger.LogWarning("Empty or null data provided for chart generation");
                throw new CustomToolException(
                    "At least one data point is required to generate a chart. " +
                    "Provide dataJson as a JSON array with objects containing 'label' and 'value' properties.");
            }

            // Validate individual data points
            for (int i = 0; i < chartDataList.Count; i++)
            {
                var dataPoint = chartDataList[i];
                if (string.IsNullOrWhiteSpace(dataPoint.Label))
                {
                    _logger.LogWarning("Data point at index {Index} has missing or empty label", i);
                    throw new CustomToolException(
                        $"Data point at index {i} has a missing or empty 'label'. All data points must have non-empty labels.");
                }
                
                if (double.IsNaN(dataPoint.Value) || double.IsInfinity(dataPoint.Value))
                {
                    _logger.LogWarning("Data point at index {Index} has invalid value: {Value}", i, dataPoint.Value);
                    throw new CustomToolException(
                        $"Data point at index {i} (label: '{dataPoint.Label}') has an invalid 'value': {dataPoint.Value}. " +
                        "Values must be valid finite numbers.");
                }

                // Special validation for pie charts - values should be non-negative
                if (normalizedChartType == "pie" && dataPoint.Value < 0)
                {
                    _logger.LogWarning("Pie chart data point at index {Index} has negative value: {Value}", i, dataPoint.Value);
                    throw new CustomToolException(
                        $"Pie chart data point at index {i} (label: '{dataPoint.Label}') has a negative value: {dataPoint.Value}. " +
                        "Pie charts require non-negative values as they represent proportions.");
                }
            }

            // Special validation for pie charts - check if all values are zero
            if (normalizedChartType == "pie" && chartDataList.All(d => d.Value == 0))
            {
                _logger.LogWarning("All pie chart values are zero");
                throw new CustomToolException(
                    "All data values are zero. Pie charts cannot be generated when all values are zero since they represent proportions.");
            }

            // Normalize title
            var normalizedTitle = string.IsNullOrWhiteSpace(title) ? "Chart" : title.Trim();

            // Create chart request
            var chartRequest = new ChartRequest
            {
                ChartType = normalizedChartType,
                Title = normalizedTitle,
                XLabel = string.IsNullOrWhiteSpace(xLabel) ? null : xLabel.Trim(),
                YLabel = string.IsNullOrWhiteSpace(yLabel) ? null : yLabel.Trim(),
                Data = chartDataList
            };

            _logger.LogInformation(
                "Generating {ChartType} chart with title '{Title}' and {DataCount} data points",
                normalizedChartType, normalizedTitle, chartDataList.Count);

            // Generate chart
            var result = await _chartService.GenerateChart(chartRequest);

            // refactor this log line to log info contained in result
            _logger.LogInformation(
                "Successfully generated {ChartType} as a {fileType} file, uploaded with name {fileName} with id {fileId}.",
                normalizedChartType, result.ContentType, result.FileName, result.Id);

            var ret = _mapper.Map<FileMetadataDTO>(result);

            return ret;
        }
        catch (CustomToolException)
        {
            throw; // Re-throw custom tool exceptions as-is
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in GenerateChartAsync: {ErrorMessage}", ex.Message);
            throw new CustomToolException(
                "An unexpected error occurred while generating the chart. Please check your input parameters and try again.", 
                ex);
        }
    }
}
