using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using AiMeetingAssistant.Desktop.Platforms.Windows;
using AiMeetingAssistant.Desktop.Services.Abstract;
using AiMeetingAssistant.Desktop.Services.Concrete;

namespace AiMeetingAssistant.Desktop;

public partial class App : Application
{
    private readonly ScreenCaptureFileStorageService _screenCaptureStorageService = new();
    private IGlobalHotkeyService? _globalHotkeyService;
    private IScreenCaptureService? _screenCaptureService;
    private int _isScreenCaptureInProgress;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.ShutdownMode = ShutdownMode.OnExplicitShutdown;
        }

        if (OperatingSystem.IsWindows())
        {
            _screenCaptureService = new WindowsScreenCaptureService();
            _globalHotkeyService = new GlobalHotkeyService();
            _globalHotkeyService.HotkeyPressed += HandleHotkeyPressed;
            _globalHotkeyService.Start();
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void ExitApplication(object? sender, EventArgs e)
    {
        if (_globalHotkeyService is not null)
        {
            _globalHotkeyService.HotkeyPressed -= HandleHotkeyPressed;
            _globalHotkeyService.Dispose();
        }

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.Shutdown();
        }
    }

    private void HandleHotkeyPressed(object? sender, EventArgs e)
    {
        _ = CaptureAndSaveScreenAsync();
    }

    private async Task CaptureAndSaveScreenAsync()
    {
        var screenCaptureService = _screenCaptureService;

        if (screenCaptureService is null ||
            Interlocked.Exchange(ref _isScreenCaptureInProgress, 1) != 0)
        {
            return;
        }

        try
        {
            var screenCapture = await screenCaptureService.CaptureAsync();
            await _screenCaptureStorageService.SaveAsync(screenCapture);
        }
        catch (Exception exception)
        {
            Debug.WriteLine(exception);
        }
        finally
        {
            Volatile.Write(ref _isScreenCaptureInProgress, 0);
        }
    }
}
