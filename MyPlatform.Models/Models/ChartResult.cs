namespace MyPlatformModels.Models
{
    public class ChartResult
    {
        public byte[] Image { get; set; } = Array.Empty<byte>();
        public int Width { get; set; } 
        public int Height { get; set; }
    }
}
