using AiMeetingAssistant.Desktop.Models;

namespace AiMeetingAssistant.Desktop.Services.Abstract;

public interface IScreenCaptureService
{
    Task<ScreenCapture> CaptureAsync(CancellationToken cancellationToken = default);
}
