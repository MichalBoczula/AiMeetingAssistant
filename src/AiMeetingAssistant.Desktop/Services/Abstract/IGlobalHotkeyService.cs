namespace AiMeetingAssistant.Desktop.Services.Abstract;

public interface IGlobalHotkeyService : IDisposable
{
    event EventHandler? HotkeyPressed;

    void Start();
}
