namespace MyPlatformMcpServer.DTOs
{
    public class StatisticsRatesResultDTO
    {
        public DateOnly ResultDateFrom { get; set; }
        public DateOnly ResultDateTo { get; set; }
        public string ProvisioningPhase { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the percentage value indicating the proportion of successful operations.
        /// </summary>
        public decimal? PercentangeOfSuccess { get; set; }

        /// <summary>
        /// Number of failed operations accumulated over time.
        /// </summary>
        public int? CumulativeKo { get; set; }
    }
}
