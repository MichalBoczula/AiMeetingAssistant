using AiMeetingAssistant.Desktop.Platforms.Windows;
using AiMeetingAssistant.Desktop.Services.Concrete;

namespace AiMeetingAssistant.Desktop;

public static class Program
{
    public static async Task Main()
    {
        if (!OperatingSystem.IsWindows())
        {
            throw new PlatformNotSupportedException("Manual screen capture is currently supported only on Windows.");
        }

        var screenCaptureService = new WindowsScreenCaptureService();
        var screenCaptureStorageService = new ScreenCaptureFileStorageService();

        var screenCapture = await screenCaptureService.CaptureAsync();
        await screenCaptureStorageService.SaveAsync(screenCapture);
    }
}
