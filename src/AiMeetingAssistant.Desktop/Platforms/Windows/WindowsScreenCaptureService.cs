using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using AiMeetingAssistant.Desktop.Models;
using AiMeetingAssistant.Desktop.Services.Abstract;

namespace AiMeetingAssistant.Desktop.Platforms.Windows;

[SupportedOSPlatform("windows")]
public sealed class WindowsScreenCaptureService : IScreenCaptureService
{
    private const int PrimaryScreenWidthMetric = 0;
    private const int PrimaryScreenHeightMetric = 1;

    public Task<ScreenCapture> CaptureAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var width = GetSystemMetrics(PrimaryScreenWidthMetric);
        var height = GetSystemMetrics(PrimaryScreenHeightMetric);

        if (width <= 0 || height <= 0)
        {
            throw new InvalidOperationException("The primary screen size could not be determined.");
        }

        using var bitmap = new Bitmap(width, height);
        using var graphics = Graphics.FromImage(bitmap);

        graphics.CopyFromScreen(
            sourceX: 0,
            sourceY: 0,
            destinationX: 0,
            destinationY: 0,
            blockRegionSize: bitmap.Size,
            copyPixelOperation: CopyPixelOperation.SourceCopy);

        using var stream = new MemoryStream();
        bitmap.Save(stream, ImageFormat.Png);

        return Task.FromResult(new ScreenCapture(
            Content: stream.ToArray(),
            ContentType: "image/png",
            CapturedAtUtc: DateTimeOffset.UtcNow));
    }

    [DllImport("user32.dll")]
    private static extern int GetSystemMetrics(int systemMetric);
}
