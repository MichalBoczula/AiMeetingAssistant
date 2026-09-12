namespace AiMeetingAssistant.Desktop.Models;

public sealed record ScreenCapture(
    byte[] Content,
    string ContentType,
    DateTimeOffset CapturedAtUtc);
