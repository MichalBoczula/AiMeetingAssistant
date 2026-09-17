using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using AiMeetingAssistant.Desktop.Platforms.Windows;
using AiMeetingAssistant.Desktop.Services.Abstract;
using AiMeetingAssistant.Desktop.Services.Concrete;
using Microsoft.Extensions.Configuration;

namespace AiMeetingAssistant.Desktop;

public partial class App : Application
{
    private const string AnalyzeScreenshotEndpointEnvironmentVariable = "AI_MEETING_ASSISTANT_ANALYZE_SCREENSHOT_ENDPOINT";

    private readonly HttpClient _httpClient = new();
    private readonly ScreenCaptureFileStorageService _screenCaptureStorageService = new();
    private readonly string _sessionId = Guid.NewGuid().ToString("N");
    private IGlobalHotkeyService? _globalHotkeyService;
    private IScreenCaptureService? _screenCaptureService;
    private IScreenshotAnalysisRequestSender? _screenshotAnalysisRequestSender;
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
            _screenshotAnalysisRequestSender = CreateScreenshotAnalysisRequestSender();
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

        _httpClient.Dispose();

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

            if (_screenshotAnalysisRequestSender is not null)
            {
                await _screenshotAnalysisRequestSender.SendAsync(
                    screenCapture,
                    _sessionId,
                    Guid.NewGuid().ToString("N"));
            }
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

    private IScreenshotAnalysisRequestSender? CreateScreenshotAnalysisRequestSender()
    {
        var endpoint = Environment.GetEnvironmentVariable(AnalyzeScreenshotEndpointEnvironmentVariable);

        if (string.IsNullOrWhiteSpace(endpoint))
        {
            endpoint = new ConfigurationBuilder()
                .AddJsonFile(Path.Combine(AppContext.BaseDirectory, "appsettings.json"), optional: true)
                .Build()[AnalyzeScreenshotEndpointEnvironmentVariable];
        }

        if (string.IsNullOrWhiteSpace(endpoint))
        {
            Debug.WriteLine($"The {AnalyzeScreenshotEndpointEnvironmentVariable} setting is not configured.");
            return null;
        }

        if (!Uri.TryCreate(endpoint, UriKind.Absolute, out var analyzeScreenshotEndpoint))
        {
            Debug.WriteLine($"The {AnalyzeScreenshotEndpointEnvironmentVariable} setting is invalid.");
            return null;
        }

        return new FunctionScreenshotAnalysisRequestSender(_httpClient, analyzeScreenshotEndpoint);
    }
}
