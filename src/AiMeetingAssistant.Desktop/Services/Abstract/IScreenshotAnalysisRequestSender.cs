using AiMeetingAssistant.Desktop.Models;

namespace AiMeetingAssistant.Desktop.Services.Abstract;

public interface IScreenshotAnalysisRequestSender
{
    Task SendAsync(
        ScreenCapture screenCapture,
        string sessionId,
        string requestId,
        CancellationToken cancellationToken = default);
}
