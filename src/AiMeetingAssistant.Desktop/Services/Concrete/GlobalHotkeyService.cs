using SharpHook;
using SharpHook.Data;
using AiMeetingAssistant.Desktop.Services.Abstract;

namespace AiMeetingAssistant.Desktop.Services.Concrete;

public sealed class GlobalHotkeyService : IGlobalHotkeyService
{
    public const string ShortcutDisplayText = "Left Ctrl + Left Alt + Left Shift + A";

    private static readonly KeyCode[] RequiredModifierKeys =
    [
        KeyCode.VcLeftControl,
        KeyCode.VcLeftAlt,
        KeyCode.VcLeftShift
    ];

    private readonly SimpleGlobalHook _globalHook = new();
    private readonly HashSet<KeyCode> _pressedKeys = [];
    private readonly object _syncRoot = new();
    private bool _isShortcutKeyPressed;
    private bool _isDisposed;
    private Task? _hookTask;

    public GlobalHotkeyService()
    {
        _globalHook.KeyPressed += HandleKeyPressed;
        _globalHook.KeyReleased += HandleKeyReleased;
    }

    public event EventHandler? HotkeyPressed;

    public void Start()
    {
        ThrowIfDisposed();

        if (_globalHook.IsRunning)
        {
            return;
        }

        _hookTask = _globalHook.RunAsync(
            globalHookType: GlobalHookType.Keyboard,
            useBackgroundThread: true);
    }

    public void Dispose()
    {
        if (_isDisposed)
        {
            return;
        }

        _globalHook.KeyPressed -= HandleKeyPressed;
        _globalHook.KeyReleased -= HandleKeyReleased;
        _globalHook.Dispose();

        _isDisposed = true;
    }

    private void HandleKeyPressed(object? sender, KeyboardHookEventArgs eventArgs)
    {
        var shouldRaiseHotkeyPressed = false;

        lock (_syncRoot)
        {
            _pressedKeys.Add(eventArgs.Data.KeyCode);

            if (eventArgs.Data.KeyCode == KeyCode.VcA &&
                !_isShortcutKeyPressed &&
                RequiredModifierKeys.All(_pressedKeys.Contains))
            {
                _isShortcutKeyPressed = true;
                shouldRaiseHotkeyPressed = true;
            }
        }

        if (shouldRaiseHotkeyPressed)
        {
            HotkeyPressed?.Invoke(this, EventArgs.Empty);
        }
    }

    private void HandleKeyReleased(object? sender, KeyboardHookEventArgs eventArgs)
    {
        lock (_syncRoot)
        {
            _pressedKeys.Remove(eventArgs.Data.KeyCode);

            if (eventArgs.Data.KeyCode == KeyCode.VcA)
            {
                _isShortcutKeyPressed = false;
            }
        }
    }

    private void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
    }
}
