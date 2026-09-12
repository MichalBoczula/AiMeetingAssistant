using AiMeetingAssistant.Desktop.Platforms.Windows;
using AiMeetingAssistant.Desktop.Services.Concrete;

if (!OperatingSystem.IsWindows())
{
    throw new PlatformNotSupportedException("Manual screen capture is currently supported only on Windows.");
}

var screenCaptureService = new WindowsScreenCaptureService();
var screenCaptureStorageService = new ScreenCaptureFileStorageService();

var screenCapture = await screenCaptureService.CaptureAsync();
await screenCaptureStorageService.SaveAsync(screenCapture);
