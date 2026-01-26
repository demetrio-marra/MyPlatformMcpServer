namespace MyPlatformMcpServer.DTOs
{
    public class StatisticsResultDTO
    {
        public DateOnly ResultDateFrom { get; set; }
        public DateOnly ResultDateTo { get; set; }
        public string ProvisioningPhase { get; set; } = string.Empty;
        public int? Ok { get; set; }
        public int? Ko { get; set; }
        public int? CumulativeKo { get; set; }
        public int? Total { get; set; }
    }
}
