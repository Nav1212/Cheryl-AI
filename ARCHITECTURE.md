# PhoneAgent Architecture

## Overview

This is a C# backend scaffolding service for an LLM-powered phone agent system. It provides the infrastructure for handling phone calls and emails but does NOT include the LLM integration or decision-making logic.

## System Components

### 1. Service Interfaces

#### IPhoneService
Abstraction for phone system integration. Allows custom phone service implementations via dependency injection.

**Methods:**
- `InitiateCallAsync(phoneNumber, sessionId)` - Start outbound calls
- `EndCallAsync(sessionId)` - Terminate calls
- `SendAudioAsync(sessionId, audioData)` - Send audio to active call
- `GetCallStatusAsync(sessionId)` - Query call status

**Implementations:**
- `DummyPhoneService` - Basic logging implementation
- `TestingPhoneService` - Enhanced testing with call session tracking

#### ISpeechToTextService
Converts audio to text.

**Methods:**
- `TranscribeAsync(audioData, audioFormat)` - Transcribe audio bytes
- `TranscribeStreamAsync(audioStream, audioFormat)` - Transcribe audio stream

**Implementation:**
- `AzureSpeechToTextService` - Azure Speech Services integration

#### ITextToSpeechService
Converts text to audio.

**Methods:**
- `SynthesizeAsync(text, voiceName)` - Generate speech from text
- `GetAvailableVoicesAsync()` - List available voices

**Implementation:**
- `AzureTextToSpeechService` - Azure Speech Services integration

### 2. API Endpoints

#### Phone API (`/api/phone`)

**POST `/api/phone/process`**
- **Purpose:** Process incoming call audio
- **Input:** PhoneCallRequest (sessionId, audioData, audioFormat)
- **Output:** PhoneCallResponse with transcribed text
- **LLM Integration Point:** LLM receives transcribedText for processing

**POST `/api/phone/respond`**
- **Purpose:** Send LLM response to caller
- **Input:** RespondRequest (sessionId, responseText, voiceName)
- **Output:** Success status
- **LLM Integration Point:** LLM calls this with its response

**POST `/api/phone/initiate`**
- **Purpose:** Start outbound call
- **Input:** InitiateCallRequest (phoneNumber)
- **Output:** sessionId

**POST `/api/phone/end`**
- **Purpose:** End active call
- **Input:** EndCallRequest (sessionId)

**GET `/api/phone/status/{sessionId}`**
- **Purpose:** Get call status

#### Email API (`/api/email`)

**POST `/api/email/process`**
- **Purpose:** Process incoming email
- **Input:** EmailRequest (emailContent, from, subject)
- **Output:** EmailResponse with LLM-generated response
- **LLM Integration Point:** LLM processes email and generates response

**POST `/api/email/analyze`**
- **Purpose:** Analyze email metadata
- **Input:** EmailRequest
- **Output:** Analysis data (sentiment, category, priority)

### 3. Data Models

- `ConversationMessage` - Chat message structure
- `EmailRequest/EmailResponse` - Email processing models
- `PhoneCallRequest/PhoneCallResponse` - Phone call models

### 4. Blazor Testing Interface

**PhoneTest.razor**
- Interactive testing page
- Microphone audio recording
- Real-time transcription
- TTS playback
- Call session management
- Event logging

**JavaScript Integration**
- `audioRecorder.js` - Browser audio capture
- Audio visualization
- Audio playback

## LLM Integration Pattern

### Phone Call Flow

```
1. User speaks → Phone System
2. Phone System → POST /api/phone/process (with audio)
3. Backend transcribes audio → returns text
4. LLM receives text → generates response
5. LLM → POST /api/phone/respond (with response text)
6. Backend converts to audio → sends to phone system
7. User hears response
```

### Email Flow

```
1. Email arrives → Email System
2. Email System → POST /api/email/process (with email content)
3. Backend logs email → returns placeholder
4. LLM processes email → generates response
5. Backend returns response to email system
6. Email system sends response to customer
```

## Dependency Injection

All services are registered in `Program.cs`:

```csharp
builder.Services.AddSingleton<IPhoneService, TestingPhoneService>();
builder.Services.AddSingleton<ISpeechToTextService, AzureSpeechToTextService>();
builder.Services.AddSingleton<ITextToSpeechService, AzureTextToSpeechService>();
```

To use your custom phone service:
```csharp
builder.Services.AddSingleton<IPhoneService, MyCustomPhoneService>();
```

## Configuration

### appsettings.json

```json
{
  "AzureSpeech": {
    "SubscriptionKey": "your-key",
    "Region": "eastus"
  },
  "PhoneAgent": {
    "DefaultVoice": "en-US-JennyNeural",
    "MaxAudioDurationSeconds": 300,
    "EnableAudioLogging": false
  }
}
```

### Environment Variables (Docker)

```
AZURE_SPEECH_KEY=your-key
AZURE_SPEECH_REGION=eastus
ASPNETCORE_ENVIRONMENT=Development
```

## Deployment

### Local Development

```bash
dotnet run
```

Access at `https://localhost:5001`

### Docker

```bash
docker-compose up
```

Access at `http://localhost:8080`

## Extension Points

### 1. Custom Phone Service
Implement `IPhoneService` to integrate your phone system:

```csharp
public class TwilioPhoneService : IPhoneService
{
    // Implement methods
}
```

### 2. Alternative STT Provider
Implement `ISpeechToTextService` for different provider:

```csharp
public class WhisperSTTService : ISpeechToTextService
{
    // Implement methods
}
```

### 3. Alternative TTS Provider
Implement `ITextToSpeechService` for different provider:

```csharp
public class ElevenLabsTTSService : ITextToSpeechService
{
    // Implement methods
}
```

## Future Enhancements

- ServiceNow integration for ticket creation
- Conversation history storage
- Multi-language support
- Call recording and analytics
- WebSocket support for real-time streaming
- Authentication and authorization
- Rate limiting
- Monitoring and telemetry
