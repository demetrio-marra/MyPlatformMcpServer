namespace MyPlatformModels.Models
{
    public class StatisticsResult
    {
        public DateOnly ResultDateFrom { get; set; }
        public DateOnly ResultDateTo { get; set; }
        public string Company { get; set; } = string.Empty;
        public string Family { get; set; } = string.Empty;
        public string Product { get; set; } = string.Empty;
        public string ProvisioningPhase { get; set; } = string.Empty;
        public int? Ok { get; set; }
        public int? Ko { get; set; }
        public int? CumulativeKo { get; set; }
        public int? Total { get; set; }
        public int? UserAverageElapsedInterval { get; set; }
    }
}