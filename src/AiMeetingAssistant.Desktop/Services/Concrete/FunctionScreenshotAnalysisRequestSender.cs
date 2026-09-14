using System.Net.Http.Headers;
using AiMeetingAssistant.Desktop.Models;
using AiMeetingAssistant.Desktop.Services.Abstract;

namespace AiMeetingAssistant.Desktop.Services.Concrete;

public sealed class FunctionScreenshotAnalysisRequestSender : IScreenshotAnalysisRequestSender
{
    private const string ScreenshotFieldName = "file";
    private const string SessionIdFieldName = "sessionId";
    private const string RequestIdFieldName = "requestId";
    private const string ScreenshotFileName = "screenshot.png";

    private readonly HttpClient _httpClient;
    private readonly Uri _analyzeScreenshotEndpoint;

    public FunctionScreenshotAnalysisRequestSender(
        HttpClient httpClient,
        Uri analyzeScreenshotEndpoint)
    {
        ArgumentNullException.ThrowIfNull(httpClient);
        ArgumentNullException.ThrowIfNull(analyzeScreenshotEndpoint);

        _httpClient = httpClient;
        _analyzeScreenshotEndpoint = analyzeScreenshotEndpoint;
    }

    public async Task SendAsync(
        ScreenCapture screenCapture,
        string sessionId,
        string requestId,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(screenCapture);
        ArgumentException.ThrowIfNullOrWhiteSpace(sessionId);
        ArgumentException.ThrowIfNullOrWhiteSpace(requestId);

        if (screenCapture.Content.Length == 0)
        {
            throw new ArgumentException("The screen capture content cannot be empty.", nameof(screenCapture));
        }

        using var multipartContent = new MultipartFormDataContent();
        using var screenshotContent = new ByteArrayContent(screenCapture.Content);

        screenshotContent.Headers.ContentType = MediaTypeHeaderValue.Parse(screenCapture.ContentType);

        multipartContent.Add(screenshotContent, ScreenshotFieldName, ScreenshotFileName);
        multipartContent.Add(new StringContent(sessionId), SessionIdFieldName);
        multipartContent.Add(new StringContent(requestId), RequestIdFieldName);

        using var response = await _httpClient.PostAsync(
            _analyzeScreenshotEndpoint,
            multipartContent,
            cancellationToken);

        response.EnsureSuccessStatusCode();
    }
}
