# Quick Start Guide

## Prerequisites

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- (Optional) [Docker Desktop](https://www.docker.com/products/docker-desktop)
- (Optional) Azure Speech Services subscription

## 1. Run Locally (Without Azure Speech)

The system works without Azure credentials - it will use dummy implementations.

```bash
# Navigate to project directory
cd PhoneAgent

# Restore dependencies
dotnet restore

# Run the application
dotnet run
```

Access the test interface at: `https://localhost:5001`

## 2. Run Locally (With Azure Speech)

### Get Azure Speech Credentials

1. Go to [Azure Portal](https://portal.azure.com)
2. Create a "Speech service" resource
3. Copy the **Subscription Key** and **Region**

### Configure Credentials

Edit `appsettings.Development.json`:

```json
{
  "AzureSpeech": {
    "SubscriptionKey": "YOUR_KEY_HERE",
    "Region": "eastus"
  }
}
```

### Run

```bash
dotnet run
```

## 3. Run with Docker

### Build and Run

```bash
# Build the Docker image
docker build -t phoneagent .

# Run with docker-compose
docker-compose up
```

### With Azure Credentials

Create `.env` file:

```bash
AZURE_SPEECH_KEY=your_key_here
AZURE_SPEECH_REGION=eastus
```

Then run:

```bash
docker-compose up
```

Access at: `http://localhost:8080`

## 4. Test the System

### Using Blazor Interface

1. Navigate to `https://localhost:5001` (or `http://localhost:8080` for Docker)
2. Click **"Start Call"** to create a test session
3. Click **"Start Recording"** and speak into your microphone
4. Click **"Stop Recording"** to transcribe your speech
5. Enter a simulated LLM response in the text box
6. Click **"Convert to Speech & Send"** to hear the response

### Using API Endpoints

**Test Speech-to-Text:**

```bash
curl -X POST https://localhost:5001/api/phone/process \
  -H "Content-Type: application/json" \
  -d '{
    "sessionId": "test-123",
    "audioData": "BASE64_AUDIO_HERE",
    "audioFormat": "wav"
  }'
```

**Test Text-to-Speech:**

```bash
curl -X POST https://localhost:5001/api/phone/respond \
  -H "Content-Type: application/json" \
  -d '{
    "sessionId": "test-123",
    "responseText": "Hello, how can I help you today?",
    "voiceName": "en-US-JennyNeural"
  }'
```

**Test Email Processing:**

```bash
curl -X POST https://localhost:5001/api/email/process \
  -H "Content-Type: application/json" \
  -d '{
    "emailContent": "I need help with my account",
    "from": "customer@example.com",
    "subject": "Support Request"
  }'
```

### View API Documentation

Navigate to `https://localhost:5001/swagger` to see all available endpoints.

## 5. Integrate Your LLM

### Phone Call Integration

Your LLM should:

1. **Listen for transcribed text** from `POST /api/phone/process`
2. **Process the text** and generate a response
3. **Send response** to `POST /api/phone/respond`

Example LLM integration code (pseudocode):

```python
# When phone call comes in
response = requests.post(
    "https://your-backend/api/phone/process",
    json={"sessionId": session_id, "audioData": audio_base64, "audioFormat": "wav"}
)

transcribed_text = response.json()["transcribedText"]

# Process with LLM
llm_response = your_llm.generate_response(transcribed_text)

# Send back to user
requests.post(
    "https://your-backend/api/phone/respond",
    json={"sessionId": session_id, "responseText": llm_response}
)
```

### Email Integration

Your LLM should:

1. **Receive email** via `POST /api/email/process`
2. **Generate response**
3. **Return response** to be sent back

## 6. Replace Dummy Phone Service

When you're ready to integrate your real phone system:

1. Create your implementation:

```csharp
public class MyPhoneService : IPhoneService
{
    public async Task<bool> InitiateCallAsync(string phoneNumber, string sessionId)
    {
        // Your phone system integration
    }

    // Implement other methods...
}
```

2. Register in `Program.cs`:

```csharp
builder.Services.AddSingleton<IPhoneService, MyPhoneService>();
```

## Troubleshooting

### "Cannot access microphone" in browser
- Grant microphone permissions when prompted
- Use HTTPS (required for microphone access)

### Azure Speech errors
- Verify your subscription key and region
- Check Azure Speech service quota
- Ensure your key is active

### Build errors
```bash
dotnet clean
dotnet restore
dotnet build
```

### Docker issues
```bash
docker-compose down
docker-compose build --no-cache
docker-compose up
```

## Next Steps

- Read [README.md](README.md) for full documentation
- Read [ARCHITECTURE.md](ARCHITECTURE.md) for system design
- Integrate your LLM with the API endpoints
- Implement your custom phone service
- Add authentication and security
- Set up monitoring and logging
