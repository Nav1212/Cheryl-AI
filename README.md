# PhoneAgent - Backend Scaffolding

A C# backend service that provides the infrastructure for an LLM-powered phone agent system. This service handles speech-to-text, text-to-speech, phone call management, and email processing.

## Architecture

This system provides the **scaffolding** for an AI phone agent, but does **NOT** include:
- LLM integration (handled externally)
- ServiceNow ticket generation (to be added later)
- Decision-making logic (handled by the LLM)

### What This System Does

1. **Speech Processing**
   - Converts incoming audio (speech) to text using Azure Speech Services
   - Converts LLM response text back to speech using Azure Speech Services

2. **Phone Service Abstraction**
   - Interface for phone system integration (`IPhoneService`)
   - Dummy and testing implementations included
   - Designed for dependency injection of your custom phone system

3. **API Endpoints for LLM**
   - `/api/phone/process` - Process incoming phone call audio, return transcribed text
   - `/api/phone/respond` - Convert LLM response to speech and send to caller
   - `/api/phone/initiate` - Start outbound calls
   - `/api/phone/end` - End active calls
   - `/api/email/process` - Process incoming emails, return placeholder for LLM response

4. **Testing Interface**
   - Blazor web interface for testing the phone agent
   - Record audio through your microphone
   - See transcriptions, simulate LLM responses, and hear text-to-speech output

## Project Structure

```
PhoneAgent/
├── Controllers/          # API endpoints
│   ├── PhoneController.cs
│   └── EmailController.cs
├── Interfaces/           # Service contracts
│   ├── IPhoneService.cs
│   ├── ISpeechToTextService.cs
│   └── ITextToSpeechService.cs
├── Services/             # Service implementations
│   ├── DummyPhoneService.cs
│   ├── TestingPhoneService.cs
│   ├── AzureSpeechToTextService.cs
│   └── AzureTextToSpeechService.cs
├── Models/               # Data models
├── Components/           # Blazor components
│   └── PhoneTest.razor
├── Pages/                # Blazor pages
└── wwwroot/              # Static files
    └── js/
        └── audioRecorder.js
```

## Getting Started

### Prerequisites

- .NET 9.0 SDK
- Docker (optional, for containerized deployment)
- Azure Speech Services subscription (for speech-to-text and text-to-speech)

### Configuration

1. Copy `.env.template` to `.env`:
   ```bash
   cp .env.template .env
   ```

2. Edit `.env` and add your Azure Speech Services credentials:
   ```
   AZURE_SPEECH_KEY=your_key_here
   AZURE_SPEECH_REGION=eastus
   ```

3. Or update `appsettings.Development.json`:
   ```json
   {
     "AzureSpeech": {
       "SubscriptionKey": "your_key_here",
       "Region": "eastus"
     }
   }
   ```

### Running Locally

1. Build the project:
   ```bash
   dotnet build
   ```

2. Run the application:
   ```bash
   dotnet run
   ```

3. Access the application:
   - Blazor Test Interface: `https://localhost:5001`
   - Swagger API Docs: `https://localhost:5001/swagger`

### Running with Docker

1. Build the Docker image:
   ```bash
   docker build -t phoneagent .
   ```

2. Run with docker-compose:
   ```bash
   docker-compose up
   ```

3. Access the application:
   - Application: `http://localhost:8080`

## API Endpoints

### Phone API (`/api/phone`)

#### POST `/api/phone/process`
Process incoming phone call audio and transcribe to text.

**Request:**
```json
{
  "sessionId": "unique-session-id",
  "audioData": "base64-encoded-audio",
  "audioFormat": "wav"
}
```

**Response:**
```json
{
  "sessionId": "unique-session-id",
  "transcribedText": "Hello, I need help with...",
  "responseText": "",
  "responseAudioData": "",
  "success": true
}
```

#### POST `/api/phone/respond`
Convert LLM response text to speech and send to caller.

**Request:**
```json
{
  "sessionId": "unique-session-id",
  "responseText": "I'd be happy to help you with that...",
  "voiceName": "en-US-JennyNeural"
}
```

**Response:**
```json
{
  "success": true,
  "audioLength": 12345,
  "sessionId": "unique-session-id"
}
```

#### POST `/api/phone/initiate`
Start an outbound call.

**Request:**
```json
{
  "phoneNumber": "+1234567890"
}
```

#### POST `/api/phone/end`
End an active call.

**Request:**
```json
{
  "sessionId": "unique-session-id"
}
```

### Email API (`/api/email`)

#### POST `/api/email/process`
Process incoming email and return response.

**Request:**
```json
{
  "emailContent": "Email body text here...",
  "from": "customer@example.com",
  "subject": "Support Request"
}
```

**Response:**
```json
{
  "response": "[LLM will process and provide response]",
  "success": true
}
```

## Testing with Blazor Interface

1. Navigate to `https://localhost:5001`
2. Click "Start Call" to initiate a test session
3. Click "Start Recording" and speak into your microphone
4. Click "Stop Recording" to transcribe your speech
5. Enter a simulated LLM response in the text box
6. Click "Convert to Speech & Send" to hear the response

## Dependency Injection

The phone service is injected via dependency injection. To use your custom implementation:

1. Create a class that implements `IPhoneService`:
   ```csharp
   public class MyPhoneService : IPhoneService
   {
       // Implement methods
   }
   ```

2. Register it in `Program.cs`:
   ```csharp
   builder.Services.AddSingleton<IPhoneService, MyPhoneService>();
   ```

## Integration with LLM

This backend is designed to work with an external LLM. Typical flow:

1. **Incoming Call:**
   - Phone system sends audio to `/api/phone/process`
   - Backend returns transcribed text
   - LLM processes the text and generates a response
   - LLM calls `/api/phone/respond` with its response
   - Backend converts to speech and sends to caller

2. **Incoming Email:**
   - Email system sends content to `/api/email/process`
   - LLM processes and generates response
   - Backend returns the response to be emailed

## Next Steps

- Integrate your custom phone system by implementing `IPhoneService`
- Connect your LLM to the API endpoints
- Add ServiceNow integration for ticket generation
- Implement decision-making logic in your LLM
- Add authentication and security measures
- Set up monitoring and logging

## License

[Your License Here]
