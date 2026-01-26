namespace MyPlatformMcpServer.DTOs
{
    public class PerformanceStatisticsResultDTO
    {
        public DateOnly ResultDateFrom { get; set; }
        public DateOnly ResultDateTo { get; set; }
        public string ProvisioningPhase { get; set; } = string.Empty;
        public int? UserAverageElapsedInterval { get; set; }
    }
}
