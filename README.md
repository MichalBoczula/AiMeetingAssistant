# AI Meeting Assistant

Cross-platform, hotkey-driven meeting assistant. The future desktop agent captures the whole screen only when explicitly triggered; the API coordinates analysis and SignalR delivery; the Angular client displays the private analysis chat on a phone or second screen.

## Solution layout

- `src/AiMeetingAssistant.API` — HTTP API, SignalR hubs, authentication and endpoint mapping.
- `src/AiMeetingAssistant.Application` — use cases, DTOs and application services.
- `src/AiMeetingAssistant.Domain` — business model and abstractions.
- `src/AiMeetingAssistant.Infrastructure` — PostgreSQL persistence and external AI integrations.
- `src/AiMeetingAssistant.Desktop` — Windows/Ubuntu tray agent, global hotkey and screen-capture adapters.
- `web/ai-meeting-assistant-chat` — responsive Angular chat client.
- `tests` — automated tests, separated by layer.

## First setup after extracting

```bash
dotnet restore AiMeetingAssistant.slnx
cd web/ai-meeting-assistant-chat
npm install
```

No capture, AI, authentication, persistence, or UI behaviour is implemented in this starter. This commit intentionally creates only the structure and dependency manifests.

## Desktop screenshot delivery

The Desktop application sends a captured screen to the Function only when the following environment variable contains the complete Function endpoint:

```text
AI_MEETING_ASSISTANT_ANALYZE_SCREENSHOT_ENDPOINT=https://<function-app>/api/analyze-screenshot
```

Each request is sent as `multipart/form-data` with `file`, `sessionId` and `requestId`. The Desktop process keeps one `sessionId` for its lifetime and creates a new `requestId` per capture.
