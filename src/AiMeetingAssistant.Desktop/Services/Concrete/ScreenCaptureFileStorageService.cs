using AiMeetingAssistant.Desktop.Models;

namespace AiMeetingAssistant.Desktop.Services.Concrete;

public sealed class ScreenCaptureFileStorageService
{
    private readonly string _screensDirectory;

    public ScreenCaptureFileStorageService(string? screensDirectory = null)
    {
        _screensDirectory = screensDirectory ?? Path.Combine(AppContext.BaseDirectory, "screens");
    }

    public async Task<string> SaveAsync(
        ScreenCapture screenCapture,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(screenCapture);

        if (screenCapture.Content.Length == 0)
        {
            throw new ArgumentException("The screen capture content cannot be empty.", nameof(screenCapture));
        }

        Directory.CreateDirectory(_screensDirectory);

        var fileName = $"screen-{screenCapture.CapturedAtUtc:yyyyMMdd-HHmmssfff}.png";
        var filePath = Path.Combine(_screensDirectory, fileName);

        await File.WriteAllBytesAsync(filePath, screenCapture.Content, cancellationToken);

        return filePath;
    }
}
