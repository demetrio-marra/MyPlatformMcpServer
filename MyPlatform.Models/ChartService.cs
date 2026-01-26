using MyPlatformModels.Helpers;
using MyPlatformModels.Models;
using MyPlatformModels.Services;
using ScottPlot;

namespace MyPlatformModels
{
    public class ChartService : IChartService
    {
        private readonly IFileServerService _fileServerService;

        private const string DefaultGeneratedFileName = "chart.png";
        private const string DefaultGeneratedContentType = "image/png";

        private int _chartWidth;
        private int _chartHeight;
     


        // Color pool for charts - 30 pastel colors visible on white background (no yellows or weak colors)
        private readonly string[] colorPool = new[]
        {
            "#FFB3BA", "#FFDFBA", "#BAFFC9", "#BAE1FF", "#E0BBE4",
            "#FFDFD3", "#C9E4DE", "#D4F1F4", "#FEC8D8", "#F7CAC9",
            "#F4C2C2", "#FFD1DC", "#C7CEEA", "#B5EAD7", "#FF9AA2",
            "#FFB7B2", "#FFDAC1", "#E2F0CB", "#C1E1C1", "#AFCBFF",
            "#D3BFDB", "#FFB3D9", "#C9ADA7", "#A8DADC", "#F1C0E8",
            "#CFBAF0", "#A3C4F3", "#90DBF4", "#8EECF5", "#98F5E1"
        };

        public ChartService(IFileServerService fileServerService, int charWidth = 1200, int chartHeight = 800)
        {
            _chartWidth = charWidth;
            _chartHeight = chartHeight;
            _fileServerService = fileServerService;
        }

        public async Task<FileMetadata> GenerateChart(ChartRequest chartRequest)
        {
            return await GenerateChartFromData(chartRequest, colorPool);
        }

        #region Private Methods
        private async Task<FileMetadata> GenerateChartFromData(ChartRequest request, string[] colorPool)
        {
            var plt = new Plot();
            
            // Fix for Linux/Docker: Configure fonts that work on Linux systems
            ConfigureLinuxCompatibleFonts(plt);
            
            plt.Title(request.Title);

            switch (request.ChartType.ToLower())
            {
                case "bar":
                    var values = request.Data.Select(d => d.Value).ToArray();
                    var labels = request.Data.Select(d => d.Label).ToArray();
                    var positions = request.Data.Select((d, i) => (double)i).ToArray();

                    // Create bars individually with different colors
                    for (int i = 0; i < values.Length; i++)
                    {
                        var bar = plt.Add.Bar(positions[i], values[i]);
                        bar.Color = ScottPlot.Color.FromHex(colorPool[i % colorPool.Length]);
                    }

                    var ticks = positions.Select((pos, i) =>
                        new ScottPlot.Tick(pos, labels[i])).ToArray();
                    plt.Axes.Bottom.TickGenerator =
                        new ScottPlot.TickGenerators.NumericManual(ticks);
                    plt.Axes.Bottom.Label.Text = request.XLabel ?? "X";
                    plt.Axes.Bottom.TickLabelStyle.Rotation = -45;
                    plt.Axes.Bottom.TickLabelStyle.Alignment = Alignment.MiddleRight;
                    plt.Axes.Left.Label.Text = request.YLabel ?? "Y";
                    plt.Axes.Left.TickLabelStyle.Rotation = -45;
                    plt.Axes.Left.TickLabelStyle.Alignment = Alignment.MiddleRight;
                    break;

                case "line":
                    var xlabels = request.Data.Select(d => d.Label).ToArray();
                    var xs = Enumerable.Range(0, request.Data.Count())
                        .Select(i => (double)i).ToArray();
                    var ys = request.Data.Select(d => d.Value).ToArray();

                    var scatter = plt.Add.Scatter(xs, ys);
                    scatter.Color = ScottPlot.Color.FromHex(colorPool[0]);
                    scatter.LineWidth = 2;

                    plt.Axes.Bottom.Label.Text = request.XLabel ?? "X";
                    plt.Axes.Bottom.TickLabelStyle.Rotation = -45;
                    plt.Axes.Bottom.TickLabelStyle.Alignment = Alignment.MiddleRight;
                    plt.Axes.Bottom.SetTicks(xs, xlabels);

                    plt.Axes.Left.Label.Text = request.YLabel ?? "Y";
                    plt.Axes.Left.TickLabelStyle.Rotation = -45;
                    plt.Axes.Left.TickLabelStyle.Alignment = Alignment.MiddleRight;
                    break;

                case "pie":
                    var slices = request.Data.Select((d, i) => new ScottPlot.PieSlice
                    {
                        Value = d.Value,
                        Label = d.Label,
                        FillColor = ScottPlot.Color.FromHex(colorPool[i % colorPool.Length]),
                        LabelFontColor = ScottPlot.Color.FromHex(colorPool[i % colorPool.Length]),
                        LabelFontSize = 14,
                        LabelBold = true
                    }).ToList();

                    var pie = plt.Add.Pie(slices);
                    plt.HideGrid();
                    plt.HideAxesAndGrid();
                    break;
            }

            var bytes = plt.GetImageBytes(_chartWidth, _chartHeight, ImageFormat.Png);

            FileMetadata? uploadedFileInfo;
            using (var memoryStream = new MemoryStream(bytes))
            {
                // Generate a filename by sanitizing the chart title with .png extension
                string sanitizedFileName;
                try
                {
                    // Add .png extension to the title before sanitization
                    sanitizedFileName = FileSystemHelper.SanitizeFileName(request.Title + ".png");
                }
                catch (ArgumentException)
                {
                    // Fallback to default filename if sanitization fails
                    sanitizedFileName = DefaultGeneratedFileName;
                }

                uploadedFileInfo = await _fileServerService.UploadFileAsync(
                    sanitizedFileName,
                    DefaultGeneratedContentType,
                    memoryStream,
                    new Dictionary<string, string>
                    {
                        { "width", _chartWidth.ToString() },
                        { "height", _chartHeight.ToString() }
                    }
                );
            }

            return uploadedFileInfo;
        }

        /// <summary>
        /// Configures fonts compatible with Linux/Docker environments.
        /// This fixes the known ScottPlot issue where labels and titles are missing on Linux.
        /// </summary>
        private void ConfigureLinuxCompatibleFonts(Plot plt)
        {
            // Set font for all plot elements to use Liberation Sans (available on most Linux systems)
            // This is a fallback that works across Windows and Linux
            var fontName = "Liberation Sans";

            // Try to detect available fonts and use a safe fallback
            try
            {
                // Set default fonts for axes labels with increased font size for visibility
                plt.Axes.Bottom.Label.FontName = fontName;
                plt.Axes.Bottom.Label.FontSize = 14;
                plt.Axes.Bottom.Label.Underline = true;
                plt.Axes.Bottom.Label.UnderlineWidth = 2;
                plt.Axes.Left.Label.FontName = fontName;
                plt.Axes.Left.Label.FontSize = 14;
                plt.Axes.Left.Label.Underline = true;
                plt.Axes.Left.Label.UnderlineWidth = 2;

                // Set default fonts for tick labels
                plt.Axes.Bottom.TickLabelStyle.FontName = fontName;
                plt.Axes.Bottom.TickLabelStyle.FontSize = 12;
                plt.Axes.Left.TickLabelStyle.FontName = fontName;
                plt.Axes.Left.TickLabelStyle.FontSize = 12;
                plt.Axes.Right.TickLabelStyle.FontName = fontName;
                plt.Axes.Right.TickLabelStyle.FontSize = 12;
                plt.Axes.Top.TickLabelStyle.FontName = fontName;
                plt.Axes.Top.TickLabelStyle.FontSize = 12;

                // Set font for title with larger size for prominence
                plt.Axes.Title.Label.FontName = fontName;
                plt.Axes.Title.Label.Underline = true;
                plt.Axes.Title.Label.UnderlineWidth = 2;
                plt.Axes.Title.Label.FontSize = 18;
            }
            catch
            {
                // If Liberation Sans is not available, ScottPlot will use its default fallback
                // which should work on most systems
            }
        }
        #endregion Private Methods
    }
}