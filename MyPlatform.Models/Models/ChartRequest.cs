namespace MyPlatformModels.Models
{
    public class ChartRequest
    {
        public IEnumerable<ChartData> Data { get; set; } = new List<ChartData>();
        public string ChartType { get; set; } = "bar";
        public string Title { get; set; } = "Chart";
        public string? XLabel { get; set; } = "X";
        public string? YLabel { get; set; } = "Y";
    }

    public class ChartData
    {
        public string Label { get; set; } = "Label";
        public double Value { get; set; }
    }
}
