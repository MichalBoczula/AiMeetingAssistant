using System.Net;
using AiMeetingAssistant.Desktop.Models;
using AiMeetingAssistant.Desktop.Services.Concrete;

namespace AiMeetingAssistant.Desktop.UnitTests.Services.Concrete;

public sealed class FunctionScreenshotAnalysisRequestSenderTests
{
    [Fact]
    public async Task SendAsync_ValidScreenCapture_ShouldPostExpectedMultipartRequest()
    {
        // Arrange
        using var handler = new RecordingHttpMessageHandler(HttpStatusCode.Accepted);
        using var httpClient = new HttpClient(handler);
        var endpoint = new Uri("https://function.example/api/analyze-screenshot");
        var sender = new FunctionScreenshotAnalysisRequestSender(httpClient, endpoint);
        var screenCapture = new ScreenCapture([1, 2, 3], "image/png", DateTimeOffset.UtcNow);

        // Act
        await sender.SendAsync(screenCapture, "session-123", "request-456");

        // Assert
        Assert.Equal(HttpMethod.Post, handler.Method);
        Assert.Equal(endpoint, handler.RequestUri);
        Assert.Equal("image/png", handler.FileContentType);
        Assert.Equal([1, 2, 3], handler.FileContent);
        Assert.Equal("session-123", handler.SessionId);
        Assert.Equal("request-456", handler.RequestId);
    }

    [Fact]
    public async Task SendAsync_UnsuccessfulResponse_ShouldThrowHttpRequestException()
    {
        // Arrange
        using var handler = new RecordingHttpMessageHandler(HttpStatusCode.BadGateway);
        using var httpClient = new HttpClient(handler);
        var sender = new FunctionScreenshotAnalysisRequestSender(
            httpClient,
            new Uri("https://function.example/api/analyze-screenshot"));
        var screenCapture = new ScreenCapture([1], "image/png", DateTimeOffset.UtcNow);

        // Act
        var action = () => sender.SendAsync(screenCapture, "session-123", "request-456");

        // Assert
        await Assert.ThrowsAsync<HttpRequestException>(action);
    }

    private sealed class RecordingHttpMessageHandler(HttpStatusCode statusCode) : HttpMessageHandler
    {
        public HttpMethod? Method { get; private set; }
        public Uri? RequestUri { get; private set; }
        public byte[]? FileContent { get; private set; }
        public string? FileContentType { get; private set; }
        public string? SessionId { get; private set; }
        public string? RequestId { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            Method = request.Method;
            RequestUri = request.RequestUri;

            var content = Assert.IsType<MultipartFormDataContent>(request.Content);
            var parts = content.ToList();

            var filePart = parts.Single(part => part.Headers.ContentDisposition?.Name == "\"file\"");
            FileContent = await filePart.ReadAsByteArrayAsync(cancellationToken);
            FileContentType = filePart.Headers.ContentType?.MediaType;
            SessionId = await ReadPartAsync(parts, "sessionId", cancellationToken);
            RequestId = await ReadPartAsync(parts, "requestId", cancellationToken);

            return new HttpResponseMessage(statusCode);
        }

        private static async Task<string> ReadPartAsync(
            IEnumerable<HttpContent> parts,
            string name,
            CancellationToken cancellationToken)
        {
            var part = parts.Single(item => item.Headers.ContentDisposition?.Name == $"\"{name}\"");
            return await part.ReadAsStringAsync(cancellationToken);
        }
    }
}
